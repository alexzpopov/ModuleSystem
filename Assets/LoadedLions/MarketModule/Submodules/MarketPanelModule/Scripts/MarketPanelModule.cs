using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base.Types.Enums;
using Common.Protocol.DataTransferObjects;
using LoadedLions.ConstructionModule;
using LoadedLions.GlobalModule;
using LoadedLions.MarketModule.Submodules.MarketPanelModule;
using LoadedLions.NetModule;
using LoadedLions.PopupModule;
using UnityEngine;

namespace LoadedLions.MarketModule.MarketPanelModule
{
	public interface IMarketPanelModule
	{
		public event Action CloseButtonClick;
		public event Action Opened;
		public Task Show(MarketPanelModel model);
		public void Hide();
		public event Action GetBuyItems;
		public event Action GetSellItems;
		public event Action GetLotstems;
		public Task UpdateItems(int skip, int maxItems,bool resetStart=false);
        public void OnSellItem(ConstructionItemData data);
        public void ShowSellItem(ConstructionItemData data,Action onFinish);
    }

	public enum Tabs
	{ buy=0,sell,lots,sellconfirm
	}

	public struct MarketPanelModel
	{
		//public INetModule netModule;
        public bool closeAuto;

        public MarketPanelModel(bool state)
        {
            closeAuto = state;
        }
    }

	public class MarketPanelModule : IMarketPanelModule
	{
		public event Action CloseButtonClick;
		public event Action Opened;

		private IMarketPanelFactory _factory;
		public MarketPanelView view;
		private IMarketPanelApiHelper _marketPanelApiHelper;
		private IGlobalFactory _globalFactory;
		private MarketPanelModel _model;
        private readonly IPopupModule _popupModule;
        public event Action GetBuyItems;
		public event Action GetSellItems;
		public event Action GetLotstems;
        private ConstructionItemData _cardData;
        private Action _OnFinish;
        private const int MAXSIMILAR = 4;
        private bool waitClick;
        private readonly ITimeService _iTimeService;

        public MarketPanelModule(IMarketPanelFactory factory,IMarketPanelApiHelper marketPanelApiHelper,IGlobalFactory globalFactory,IPopupModule popupModule,ITimeService iTimeService)
		{
			_factory = factory;
			_marketPanelApiHelper = marketPanelApiHelper;
			_globalFactory = globalFactory;
            _popupModule = popupModule;
            _iTimeService = iTimeService;
        }

		public async void SetFilter(MarketFilterDTO data)
		{
			_marketPanelApiHelper.SetFilter(data,view.FilterTab);
			await UpdateItems(0,view.MAXITEMSPERPAGE);
		}

		public async Task Show(MarketPanelModel model)
		{
			_model = model;

			if(view == null)
				view = await _factory.Create();
            view.FilterSort += ApplySort;
			view.FilterApply += SetFilter;
			view.SellClick += ShowSellConfirm;
			view.CloseButtonClick += Hide;
			view.ItemCardInfoClick += ShowInfo;
			view.gameObject.SetActive(true);
			view._tabButtons[(int)Tabs.buy].onValueChanged.AddListener((state) => { OnTabChanged(state); });
			view._tabButtons[(int)Tabs.sell].onValueChanged.AddListener((state) => { OnTabChanged(state); });
			view._tabButtons[(int)Tabs.lots].onValueChanged.AddListener((state) => { OnTabChanged(state); });
			view.SellClickConfirm += OnSellItem;
			view.RemoveClick += OnRemoveItem;
            view.BuyClick += OnBuyItem;

			_marketPanelApiHelper.BackEndRespond += BackEndRespond;
			view.SellConfirmPanel.OnClose += () => { view.ShowTab(Tabs.sell); };
            view.paginatorView.ValueChange += PaginatorValueChange;
            if (!_model.closeAuto)
            {
                await UpdateItems(0,view.MAXITEMSPERPAGE);
            }
			Opened?.Invoke();
		}

        private async void OnBuyItem(ConstructionItemData data)
        {
            if (waitClick)
            {
                Debug.Log("Double click");
                return;
            }
            _cardData = data;
            waitClick = true;
            await _popupModule.Show(new PopupModel
            {
                type = PopupType.Question,
                title = "Are you sure?",
                text = "You are buying "+data.Name+" for <sprite=1> " + ((decimal)data.Cost).PringBig(),
                firstButton = "CONFIRM",
                secondButton="CANCEL"
                //Coin icon: <sprite=0>;
            });
            _popupModule.Response +=BuyConfirmed;
        }

        private async void BuyConfirmed(bool state)
        {
            _popupModule.Response -=BuyConfirmed;
            if (state)
            await _marketPanelApiHelper.CheckBuyItem(_cardData);
            waitClick = false;
        }

        private async void PaginatorValueChange(int value)
        {
            await UpdateItems(value,view.MAXITEMSPERPAGE);
        }
		private async void BackEndRespond()
		{
            view.InfoPanel.ShowInfo(false);
			await UpdateItems(0,view.MAXITEMSPERPAGE);
		}

        public async void OnSellItem(ConstructionItemData data)
        {
            _cardData = data;
            await _popupModule.Show(new PopupModel
            {
                type = PopupType.Question,
                title = "Are you sure",
                text = "You are selling "+data.Name+" for <sprite=1> " + data.Cost.ToString(),
                firstButton = "CONFIRM",
                secondButton="CANCEL"
                //Coin icon: <sprite=0>;
            });
            _popupModule.Response +=SellConfirmed;
        }

        private async void SellConfirmed(bool state)
        {
            _popupModule.Response -=SellConfirmed;
            if (state)
            {
                await _marketPanelApiHelper.SellItem(_cardData);
            }

            if (_model.closeAuto)
            {
                Hide();
                _OnFinish?.Invoke();
            }
            else
            {
                view.ShowTab(Tabs.sell);
            }
        }

		private void OnRemoveItem(ConstructionItemData data)
		{
			_marketPanelApiHelper.LotdItem(data);
		}

		private async void OnTabChanged(bool state)
		{
			if (state)
			{
                _marketPanelApiHelper.SetFilter(new MarketFilterDTO(),0);
				await UpdateItems(0,view.MAXITEMSPERPAGE,true);
			}
		}

        public async void ShowSellItem(ConstructionItemData cardData,Action OnFinish)
        {
            _OnFinish = OnFinish;
            ShowInfo(cardData);
            ShowSellConfirm();
            view.SellConfirmPanel.OnClose += AutoClose;
        }

        private void AutoClose()
        {
            view.InfoPanel.OnClose -= AutoClose;
            _OnFinish?.Invoke();
            Hide();
        }
		public async void ShowSellConfirm()
        {
            view.ShowTab(Tabs.sellconfirm);
            await ShowSimilarBuilting();
		}

        public async void ShowInfo(ConstructionItemData cardData)
        {
            ShowInfo(view.InfoPanel, cardData);
            ShowInfo(view.SellConfirmPanel, cardData);
            view.InfoPanel.ShowInfo(true);
        }
        private async Task ShowSimilarBuilting()
        {
            var baseItem = view.SellConfirmPanel.Data;
            var filter = new MarketFilterDTO()
            {
                Category =  (baseItem.Type==ConstructionItemData.CardType.Blueprint) ? MarketItemType.Blueprint : MarketItemType.BusinessBuilding,
                MinLevel = baseItem.Level
            };


            var requestBuy = await _marketPanelApiHelper.GetBuyRequest(filter,MarketSort.None,0,MAXSIMILAR);

            foreach (var item in view.PoolSellItemViews)
            {
                item.gameObject.SetActive(false);
            }

            view.ShowNoitemsSimilar(requestBuy.Buildings.Count == 0);
            for (int i = 0; i < Mathf.Min(requestBuy.Buildings.Count,MAXSIMILAR); i++)
            {
                //debug
                var data=await _marketPanelApiHelper.GetBuildingByDTO(requestBuy.Buildings[i], requestBuy.Items);
                view.PoolSellItemViews[i].SetData(data);
                await view.SetupMarketSimilarCardView(view.PoolSellItemViews[i], data);

                view.PoolSellItemViews[i].gameObject.SetActive(true);
            }



        }
		private async void ShowInfo(MarketInfoCard card, ConstructionItemData cardData)
		{
            card.SetTime(_iTimeService.ShiftTimeShiftOnly);
			card.SetData(cardData);
			card.Setup(cardData.Name,
                (cardData.Type==ConstructionItemData.CardType.Blueprint) ?
                    await _globalFactory.GetSpriteByAssetPath(cardData.ImagePath)
                    :
					_globalFactory.GetBuildingSpriteById(cardData.Id),
					cardData.IsSelected,
					cardData.Rarity,
					true,
					null,
					cardData.Rarity.ToString(),
					goldGenerationRate:cardData.GoldGenerationRate,
					timeCreated:cardData.CreatedTime,
					level:cardData.Level,
					cost:cardData.Cost);

		}

        public async void ApplySort()
        {
            await UpdateItems(0, view.MAXITEMSPERPAGE);
        }
		public async Task UpdateItems(int skip, int maxItems,bool resetStart=false)
		{
			if (view.BusyUpdateView)
				return;
            if (resetStart)
            {
                view.paginatorView._startpoint = 0;
            }
			var tab = view.CurrentTab;
            if (tab == 3)
            {
                tab = 1;
            }
            ItemsResult itemsResult= new ItemsResult() {constructionItemData=new List<ConstructionItemData>(),totalItemsCount=0};
			switch (tab)
			{
				case 0:
                    itemsResult = await _marketPanelApiHelper.LoadMarketItems((MarketTabs.buy),view.SortFilter,skip+view.paginatorView._startpoint, maxItems);
					break;
				case 1:
                    itemsResult = await _marketPanelApiHelper.LoadMarketItems((MarketTabs.sell),view.SortFilter,skip+view.paginatorView._startpoint, maxItems);
					break;
				case 2:
                    itemsResult = await _marketPanelApiHelper.LoadMarketItems((MarketTabs.lots),view.SortFilter,skip+view.paginatorView._startpoint, maxItems);
					break;
			}

            itemsResult.skip = skip;
			await view.SetupItems(itemsResult);
            view.BusyUpdateView = false;
        }
		public void Hide()
		{
			view.CloseButtonClick -= Hide;
            _factory.Release(view.gameObject);
        }
	}
}
