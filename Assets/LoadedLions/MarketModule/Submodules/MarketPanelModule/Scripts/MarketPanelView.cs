using System;
using System.Collections.Generic;
using Common.Base.Types.Enums;
using Common.Protocol.DataTransferObjects;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using LoadedLions.ConstructionModule;
using LoadedLions.GlobalModule;
using LoadedLions.MarketModule.MarketPanelModule;
using LoadedLions.UtilitiesModule.MinmaxSlider;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using ItemRarity = Common.Base.Types.Enums.ItemRarity;
using Toggle = UnityEngine.UI.Toggle;

namespace LoadedLions.MarketModule.Submodules.MarketPanelModule
{
    public class MarketPanelView : MonoBehaviour
    {
        public event Action CloseButtonClick;
        public event Action InfoPanelsButtonClick;
        public event Action FilterPanelsButtonClick;
        public event Action<ConstructionItemData> BuyClick;
        public event Action SellClick;

        public event Action<ConstructionItemData> RemoveClick;

        //after confirm
        public event Action<ConstructionItemData> SellClickConfirm;

        public event Action<ConstructionItemData> ItemCardInfoClick;
        public event Action<MarketFilterDTO> FilterApply;
        public event Action FilterSort;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Toggle[] _panelsButton;
        [SerializeField] private GameObject[] _panels;
        [SerializeField] public Toggle[] _tabButtons;
        [SerializeField] private GameObject[] _tabs;
        [SerializeField] private GameObject[] _tabsHeader;
        [SerializeField] private bool[] _showinfo;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _sellConfirm;
        [SerializeField] public RectTransform content;
        [SerializeField] public RectTransform sellContent;
        [SerializeField] private int currentTab;

        [SerializeField] private Toggle sort;
        [SerializeField] private GameObject _loading;
        [SerializeField] private GameObject _loadingProgress;
     //   [SerializeField] private GameObject _allHolder;
        [SerializeField] private GameObject[] _filterPanels;
        [SerializeField] private GameObject _noitem;
        [SerializeField] private TMP_Text _itemcountText;
        [SerializeField] private TMP_Dropdown _sort;
        [SerializeField] private Toggle _sortDir;
        [SerializeField] protected GameObject sellNoitems;
        private MarketSort _sortFilter;
        private Sequence mySequence;
        public int CurrentTab => currentTab;
        private int _filterTab;
        public int FilterTab => _filterTab;
        public MarketSort SortFilter => _sortFilter;

        public bool BusyUpdateView
        {
            get => _busyUpdateView;
            set
            {
                _busyUpdateView = value;
                if (_loading)
                {
                    _loading.SetActive(value);
                    mySequence = DOTween.Sequence();
                    mySequence.Append(_loadingProgress.transform
                        .DORotate(new Vector3(0, 0, 360), 10, RotateMode.FastBeyond360)
                        .SetLoops(-1, LoopType.Restart)
                        .SetEase(Ease.Linear));
                }
            }
        }

        private IGlobalFactory _globalFactory;

        private IRarityService _rarityService;

        [SerializeField] private GameObject[] _businessGroup;

        //filter
        [SerializeField] private Toggle[] Categories;
        [SerializeField] private Toggle[] ItemRarity;
        [SerializeField] private Toggle[] ItemRarityBlueprint;
        [SerializeField] private MinMaxSlider level;
        [SerializeField] private MinMaxSlider income;

        [SerializeField] private Button ApplyButton;

        private bool _busyUpdateView;

        public List<ConstructionItemCardView> PoolItemViews = new List<ConstructionItemCardView>();

        public List<MarketItemCard> PoolSellItemViews = new List<MarketItemCard>();

        //info
        public MarketInfoCard InfoPanel;
        public MarketInfoCard SellConfirmPanel;

        //paginator
        [HideInInspector] public int MAXPOINTS = 6;
        private const int MAXSELLVARIANT = 4;
        public int MAXITEMSPERPAGE;
        public int totalItemsCount;

        public MarketPaginatorView paginatorView;

        public void ShowNoitemsSimilar(bool state)
        {
        if (sellNoitems)
        {
            sellNoitems.SetActive(state);
        }
        }

        private MarketFilterDTO Filter()
        {

            var filter = new MarketFilterDTO();
            filter.Category = new MarketItemType();
            int index = 0;
            foreach (var item in Categories)
            {
                if (item.isOn)
                {
                    filter.Category = (MarketItemType)index;
                }

                index++;
            }

            filter.Qualities = new List<ItemRarity>();

            switch (FilterTab)
            {
                case 1:
                    SetRarity();
                    break;
                case 2:
                    SetRarityBlueprint();
                    break;
            }

            filter.MinIncome = (int)income.Values.minValue;
            filter.MaxIncome = (int)income.Values.maxValue;
            filter.MinLevel = (int)level.Values.minValue;
            filter.MaxLevel = (int)level.Values.maxValue;
            return filter;

            void SetRarity()
            {
                index = 1;
                foreach (var item in ItemRarity)
                {
                    if (item.isOn)
                    {
                        filter.Qualities.Add((ItemRarity)index);
                    }
                    index++;
                }
            }
            void SetRarityBlueprint()
            {
                if (ItemRarityBlueprint[0].isOn)
                {
                    filter.Qualities.Add((ItemRarity)1);
                }
                if (ItemRarityBlueprint[1].isOn)
                {
                    filter.Qualities.Add((ItemRarity)2);
                }
                if (ItemRarityBlueprint[2].isOn)
                {
                    filter.Qualities.Add((ItemRarity)1);
                    filter.Qualities.Add((ItemRarity)2);
                }

            }

        }

        private void OnSortClick(int state)
        {
            state = _sort.value + 1;// state none don't use
            _sortFilter = MarketSort.None;
            switch (state)
            {
                case 0:
                    _sortFilter = MarketSort.None;
                    break;
                case 1:
                    _sortFilter = _sortDir.isOn ? MarketSort.PriceAscending : MarketSort.PriceDescending;
                    break;
                case 2:
                    _sortFilter = _sortDir.isOn ? MarketSort.CreateTimeAscending : MarketSort.CreateTimeDescending;
                    break;
            }

            FilterSort?.Invoke();
        }

        void Awake()
        {
            MAXITEMSPERPAGE = 6;
            income.SetLimits(0, 10000);
            level.SetLimits(0, 999);
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _sort.onValueChanged.AddListener(OnSortClick);
            _sortDir.onValueChanged.AddListener((state) => OnSortClick(0));
            _panelsButton[0].onValueChanged.AddListener((state) =>
            {
                if (state)
                {
                    InfoPanelsButtonClick?.Invoke();
                    ShowByID(_panels, 0);
                }

                ;
            });
            _panelsButton[0].onValueChanged.AddListener((state) =>
            {
                if (!state)
                {
                    FilterPanelsButtonClick?.Invoke();
                    ShowByID(_panels, 1);
                }
            });

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int j = i;
                _tabButtons[i].onValueChanged.AddListener((state) =>
                {
                    if (state)
                    {
                        ShowTab((Tabs)j);
                    }
                });
            }

            _buyButton.onClick.AddListener(() => BuyClick?.Invoke(InfoPanel.Data));
            _sellButton.onClick.AddListener(() => SellClick?.Invoke());
            _addButton.onClick.AddListener(() => RemoveClick?.Invoke(InfoPanel.Data));
            _sellConfirm.onClick.AddListener(() => { SellClickConfirm?.Invoke(SellConfirmPanel.Data); });
            ApplyButton.onClick.AddListener(OnFilterApplyClick);
            for (int i = 0; i < Categories.Length; i++)
            {
                int index = i;
                Categories[i].onValueChanged.AddListener((state)=>
                {
                    if (state)
                        ShowHeaders(index);
                });
            }

            ShowHeaders(0);

        }

        private void ShowHeaders(int index)
        {
            _filterTab = index;
                for (int i = 0; i < _filterPanels.Length; i++)
                {
                    _filterPanels[i].SetActive(i == index);
                }
        }

        public void ShowTab(Tabs index)
        {
            currentTab = (int)index;
            InfoPanel.ShowInfo(false);
            ShowByID(_tabs, (int)index);
            ShowByID(_tabsHeader, (int)index);
            if (_showinfo[(int)index])
            {
                ShowByID(_panels, 0);
            }

        }

        public void Init(IGlobalFactory globalFactory, IRarityService rarityService)
        {
            _globalFactory = globalFactory;
            _rarityService = rarityService;
            InitVar();
        }

        private async void InitVar()
        {
            currentTab = 0;
            //  _panelsButton[0].isOn = true;
            //  _tabButtons[currentTab].isOn = true;
            InfoPanel.Init(_rarityService);
            SellConfirmPanel.Init(_rarityService);
            await FillPoolSell();

        }

        private async UniTask FillPoolSell()
        {
            for (int i = 0; i < MAXSELLVARIANT; i++)
            {
                //MarketItemCard
                var view = await _globalFactory.CreateView<MarketItemCard>("MarketItemCard", sellContent);
                view.gameObject.SetActive(false);
                view.Init(_rarityService);
                PoolSellItemViews.Add(view);
            }
        }

        private void ShowByID(GameObject[] objs, int index)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i].SetActive(i == index);
            }
        }

        private void DeleteAllItems()
        {
            foreach (var item in PoolItemViews)
            {
                item.CardClick -= OnCardClick;
                item.gameObject.SetActive(false);
            }
        }

        private async UniTask CreateAndSetupCard(ItemsResult marketItems,int i)
        {
            PoolItemViews.Add(await CreateCardView());
            await OnlySetupCard(marketItems,i);
        }

        private async UniTask OnlySetupCard(ItemsResult marketItems, int i)
        {
            var cardData = marketItems.constructionItemData[i];
            var cardView = PoolItemViews[i];
            cardView.SetData(cardData);
            await SetupCardView(cardView, cardData);
            cardView.CardClick += OnCardClick;
            cardView.gameObject.SetActive(true);
        }

        private async UniTask<ConstructionItemCardView> CreateCardView()
        {
            var view = await _globalFactory.CreateBuildingCard(content);
            view.gameObject.SetActive(false);
            view.Init(_rarityService);
            view.SetDragState(false);
            view.CardClick += OnCardClick;
            return view;
        }

        public async UniTask SetupItems(ItemsResult marketItems)
        {
            void NoItems()
            {
                _noitem.SetActive(totalItemsCount == 0);
  //              ShowHeaders(totalItemsCount != 0);
                if (totalItemsCount == 0)
                {
                    foreach (var panel in _panels)
                    {
                        panel.gameObject.SetActive(false);
                    }
                }
            }
            BusyUpdateView = true;
            DeleteAllItems();
           // paginatorView.ResetCurrentValue();
            totalItemsCount = marketItems.totalItemsCount;
            NoItems();
            _itemcountText.text = totalItemsCount.ToString();
            var pages = (totalItemsCount + MAXITEMSPERPAGE - 1) / MAXITEMSPERPAGE-1;
            paginatorView.Init(Mathf.Min(MAXPOINTS, pages), marketItems.skip, pages);

            // var setupCardTasks = new List<UniTask>();
            // for (var i = 0; i < marketItems.constructionItemData.Count; i++)
            //     setupCardTasks.Add(PoolItemViews.Count > i ? OnlySetupCard(i) : CreateAndSetupCard(i));
            // await UniTask.WhenAll(setupCardTasks);
            for (var i = 0; i < marketItems.constructionItemData.Count; i++)
                if (PoolItemViews.Count > i)
                    await OnlySetupCard(marketItems,i);
                else
                    await CreateAndSetupCard(marketItems,i);
        }

        private void OnCardClick(ItemCardViewBase<ConstructionItemData> view, ConstructionItemData data)
        {
            ItemCardInfoClick?.Invoke(data);
        }

        public async UniTask SetupCardView(ConstructionItemCardView cardView, ConstructionItemData cardData)
        {
            bool? showCro = null;
            bool showRarity = false;
            switch (currentTab)
            {
                case 0:
                    showRarity = true;
                    showCro = true;
                    break;
                case 1:
                    showRarity = cardData.Type!=ConstructionItemData.CardType.Blueprint;
                    break;
                case 2:
                    showRarity = true;
                    showCro = true;
                    break;
            }

            cardView.Setup(cardData.Name,
                (cardData.Type==ConstructionItemData.CardType.Blueprint) ?
                    await _globalFactory.GetSpriteByAssetPath(cardData.ImagePath)
                    :
                await _globalFactory.GetBuildingSpriteByDTO(cardData.DTO,
                    cardData.ImagePath),
                cardData.IsSelected,
                cardData.Rarity,
                true,
                null,
                cardData.Rarity.ToString(),
                idRarity: cardData.Rarity ,
                level:cardData.Level,
                showCro:showCro);
            cardView.ShowRarity(showRarity);//(cardData.Type!=ConstructionItemData.CardType.Blueprint));
            cardView.ShowLevel();

        }

        private async UniTask<Sprite> GetSpriteItem(int type, ConstructionItemData cardData)
        {
            Sprite res;
            res = _globalFactory.GetBuildingSpriteById(cardData.Id);
            return null;
        }

        public async UniTask SetupMarketSimilarCardView(MarketItemCard cardView, ConstructionItemData cardData)
        {
            cardView.Setup(cardData.Name,

                (cardData.Type==ConstructionItemData.CardType.Blueprint) ?
                    await _globalFactory.GetSpriteByAssetPath(cardData.ImagePath)
                    :
                await _globalFactory.GetBuildingSpriteByDTO(cardData.DTO,
                    cardData.ImagePath),
                cardData.IsSelected,
                cardData.Rarity,
                true,
                null,
                cardData.Rarity.ToString(),
                cardData.Rarity,
                cardData.Level,
                cardData.Cost
            );
        }

        private void OnDestroy()
        {
            DOTween.KillAll(_loadingProgress.gameObject);
            _sort.onValueChanged.RemoveAllListeners();
            _sortDir.onValueChanged.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            foreach (var item in _panelsButton)
            {
                item.onValueChanged.RemoveAllListeners();
            }

            foreach (var item in _tabButtons)
            {
                item.onValueChanged.RemoveAllListeners();
            }
        }

        private void OnFilterApplyClick()
        {
            FilterApply?.Invoke(Filter());
        }

        private void OnCloseButtonClick() =>
            CloseButtonClick?.Invoke();
    }
}
