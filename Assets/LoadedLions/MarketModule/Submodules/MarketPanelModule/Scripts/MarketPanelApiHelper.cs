using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common.Base.Types.Enums;
using Common.Protocol.DataTransferObjects;
using Common.Protocol.RequestAndResponse;
using Common.Storage.GridStorageItems;
using LoadedLions.ConstructionModule;
using LoadedLions.GlobalModule;
using LoadedLions.GlobalModule.BuildingCardView;
using LoadedLions.Infrastructure;
using LoadedLions.MarketModule.MarketPanelModule;
using LoadedLions.MarketModule.Submodules.MarketPanelModule;
using LoadedLions.NetModule;
using LoadedLions.NetModule.MetaMaskModule;
using Stepico.Net.GameApi.Protocol;
using TMPro;
using UnityEngine;
using ItemRarity = LoadedLions.GlobalModule.ItemRarity;
using Random = UnityEngine.Random;

namespace LoadedLions.MarketModule
{
    public class ListMarketDTO
    {
        public List<MarketItemDTO> Items;
        public List<BuildingDTO> Buildings;
        public List<BlueprintDTO> Blueprints;
        public List<BoosterDTO> Boosters;
    }

    public interface IMarketPanelApiHelper : IApiHelper
    {
        Task<ItemsResult> LoadMarketItems(MarketTabs tab, MarketSort sort, int skip, int maxItems);
        void SetFilter(MarketFilterDTO filter, int viewFilterTab);
        Task<BuyMarketItemResponse> CheckBuyItem(ConstructionItemData card);
        Task<SellMarketItemResponse> SellItem(ConstructionItemData card);
        Task<RemoveMarketItemResponse> LotdItem(ConstructionItemData card);

        Task<GetMarketBuyItemsResponse> GetBuyRequest(MarketFilterDTO filterValue, MarketSort sort, int skip,
            int getItems);

        Task<ConstructionItemData> GetBuildingByDTO(BuildingDTO dto, List<MarketItemDTO> Items);
        event Action BackEndRespond;
        void Init(IAssetProvider assetProvider);
    }

    public enum MarketTabs
    {
        buy,
        sell,
        lots
    }

    public class MarketPanelApiHelper : IMarketPanelApiHelper, IDisposable
    {
        private INetModule _netModule;
        private IMarketPanelApiHelper marketPanelApiHelperImplementation;
        private IRarityService _rarityService;
        private MarketFilterDTO _filter = null;
        public event Action BackEndRespond;
        private ConstructionItemData _card;
        private IAssetProvider _assetProvider;
        private readonly IMetaMaskModule _metaMaskModule;
        private BillingController _billingController;
        private bool _init;
        private int _viewFilterTab;

        public void Init(IAssetProvider assetProvider)
        {
            if (_init)
            {
                return;
            }

            _init = true;
            _assetProvider = assetProvider;
            _billingController = _metaMaskModule.GetBillingController();
        }

        public async Task<(bool, IEnumerable<ErrorContainer>)> Prepare() =>
            await Task.FromResult<(bool, IEnumerable<ErrorContainer>)>((true, Array.Empty<ErrorContainer>()));

        public async Task<BuyMarketItemResponse> CheckBuyItem(ConstructionItemData card)
        {
            _card = card;
            _billingController.OnCheck += AcceptBuyItem;
            _billingController.CheckBuyDiamond(card.LotdId, card.Cost);
            return null;
        }

        public async void AcceptBuyItem(bool accept)
        {
            _billingController.OnCheck -= AcceptBuyItem;
            if (accept)
            {
                await BuyItem();
            }
        }

        public async Task<BuyMarketItemResponse> BuyItem()
        {
            var lotId = await BuyItemLockCheck();
            if (lotId != null)
            {
                _billingController.BuyMarketItemCRO(_card.SellerWallet, _card.Cost, _card.LotdId.ToString());
            }

            BackEndRespond?.Invoke();

            return null;
        }

        public async Task<ConstructionItemData> BuyItemLockCheck()
        {
            string transaction = "";
            var card = _card;
            var request = new BuyMarketItemRequest();
            request.LotId = card.LotdId;
            request.PriceCRO = card.Cost;
            Debug.Log("BuyItemLockCheck LotID " + request.LotId);
            var requestAsync = await _netModule.CustomNetwork.SendRequestAsync(
                operations => operations.BuyMarketItemOp, request);
            if (!requestAsync.Success)
            {
                Debug.LogError(requestAsync.Error.DebugMessage);
                return null;
            }

            return card;
        }

        public void SetFilter(MarketFilterDTO filter, int viewFilterTab)
        {
            _viewFilterTab = viewFilterTab;
            _filter = filter;
        }

        public MarketPanelApiHelper(INetModule netModule, IRarityService rarityService, IMetaMaskModule metaMaskModule)
        {
            _netModule = netModule;
            _rarityService = rarityService;
            _metaMaskModule = metaMaskModule;
        }

        public async Task<ConstructionItemData> GetBuildingByDTO(BuildingDTO dto, List<MarketItemDTO> Items)
        {
            var grsi = BusinessBuildingsGSI.Instance.Get(dto.Type);

            var item = BuildingDtoConverter.ConvertToConstructionItemData(dto,
                grsi,
                _rarityService.GetData(RarityService.ToItemRarity(grsi.Tier)).name);
            item = await FillCost(item, Items);
            item.DTO = dto;
            return item;
        }

        private async Task<ConstructionItemData> FillCost(ConstructionItemData item, List<MarketItemDTO> itemsInfo)
        {
            if ((itemsInfo == null) || (itemsInfo.Count < 0))
            {
                return item;
            }

            MarketItemType typeFilter = MarketItemType.BusinessBuilding;
            if (item.Type == ConstructionItemData.CardType.Blueprint)
            {
                typeFilter = MarketItemType.Blueprint;
            }

            int i = (itemsInfo.FindIndex(i => i.ItemId == item.Id && i.ItemType == typeFilter));
            if (i >= 0)
            {
                item.Cost = (int)itemsInfo[i].PriceCRO;
                item.CreatedTime = (int)itemsInfo[i].CreatedTime;
                item.LotdId = itemsInfo[i].Id;
                item.SellerWallet = itemsInfo[i].SellerWallet;
            }

            return item;
        }

        public async Task<ItemsResult> LoadMarketItems(MarketTabs tab, MarketSort sort, int skip, int maxItems)
        {
            ListMarketDTO MarketItemDTO = new ListMarketDTO();
            List<ConstructionItemData> _constructionItems = new List<ConstructionItemData>();
            int totalCount = 0;
            skip *= maxItems;
            switch (tab)
            {
                case MarketTabs.buy:
                    var requestBuy = await GetBuyRequest(_filter, sort, skip, maxItems);
                    MarketItemDTO.Items = requestBuy.Items;
                    MarketItemDTO.Buildings = requestBuy.Buildings;
                    MarketItemDTO.Blueprints = requestBuy.Blueprints;
                    totalCount = requestBuy.TotalItemsCount;
                    break;
                case MarketTabs.sell:
                    var requestSell = await GetSellRequest(skip, maxItems);
                    MarketItemDTO.Buildings = requestSell.Buildings;
                    MarketItemDTO.Blueprints = requestSell.Blueprints;
                    totalCount = requestSell.TotalItemsCount;
                    break;
                case MarketTabs.lots:
                    var requestLots = await GetLotsRequest(skip, maxItems);
                    MarketItemDTO.Items = requestLots.Items;
                    MarketItemDTO.Buildings = requestLots.Buildings;
                    MarketItemDTO.Blueprints = requestLots.Blueprints;
                    totalCount = requestLots.TotalItemsCount;
                    break;
            }

            foreach (var dto in MarketItemDTO.Buildings)
            {
                ConstructionItemData item = await GetBuildingByDTO(dto, MarketItemDTO.Items);
                item.Level = dto.Level;
                item.GoldGenerationRate = dto.GoldGenerationRate;
                _constructionItems.Add(item);
            }

            foreach (var dto in MarketItemDTO.Blueprints)
            {
                ConstructionItemData item = await GetBluePrintByDTO(dto, MarketItemDTO.Items);
                _constructionItems.Add(item);
            }

            Debug.Log("requestAsync.Response.itemsInfo getItems =" + MarketItemDTO.Buildings.Count);
            return new ItemsResult() { constructionItemData = _constructionItems, totalItemsCount = totalCount };
        }

        public async Task<ConstructionItemData> GetBluePrintByDTO(BlueprintDTO dto, List<MarketItemDTO> Items)
        {
            string spriteName = "BlueprintCommon";
            ItemRarity rarity = ItemRarity.Common;
            switch (dto.BlueprintType)
            {
                case BlueprintType.Unknown:
                    spriteName = "BlueprintCommon";
                    rarity = ItemRarity.Common;
                    break;
                case BlueprintType.Common:
                    spriteName = "BlueprintCommon";
                    rarity = ItemRarity.Common;
                    break;
                case BlueprintType.Epic:
                    spriteName = "BlueprintEpic";
                    rarity = ItemRarity.Epic;
                    break;
                default:
                    spriteName = "BlueprintCommon";
                    rarity = ItemRarity.Common;
                    break;
            }

            var item = new ConstructionItemData(ConstructionItemData.CardType.Blueprint, dto.Id, "", "BLUEPRINT",
                spriteName);
            item.Rarity = rarity;
            item = await FillCost(item, Items);
            return item;
        }

        public async Task<SellMarketItemResponse> SellItem(ConstructionItemData card)
        {
            var sell = new SellMarketItemRequest()
            {
                ItemId = card.Id,
                ItemType = card.Type == ConstructionItemData.CardType.Business
                    ? MarketItemType.BusinessBuilding
                    : MarketItemType.Blueprint, //  ItemType.Building;
                PriceCRO = card.Cost
            };

            var requestAsync = await _netModule.CustomNetwork.SendRequestAsync(
                operations => operations.SellMarketItemOp, sell);
            if (!requestAsync.Success)
            {
                Debug.Log(requestAsync.Error.DebugMessage);
            }
            else
            {
                BackEndRespond?.Invoke();
            }

            return requestAsync.Response;
        }

        public async Task<RemoveMarketItemResponse> LotdItem(ConstructionItemData card)
        {
            var remove = new RemoveMarketItemRequest { LotId = card.LotdId };

            var requestAsync = await _netModule.CustomNetwork.SendRequestAsync(
                operations => operations.RemoveMarketItemOp, remove);
            if (!requestAsync.Success)
            {
                Debug.Log(requestAsync.Error);
            }
            else
            {
                BackEndRespond?.Invoke();
            }

            return requestAsync.Response;
        }

        public async Task<GetMarketBuyItemsResponse> GetBuyRequest(MarketFilterDTO filterValue, MarketSort sort,
            int skip, int getItems)
        {
            var filter = filterValue ?? new MarketFilterDTO();
            var filterDTO = new GetMarketBuyItemsRequest()
            {
                Skip = skip, Take = getItems, Sort = sort, Filters = filter
            };
            var requestAsync = await _netModule.CustomNetwork.SendRequestAsync(
                operations => operations.GetMarketBuyItemsOp, filterDTO
            );
            if (!requestAsync.Success)
                Debug.Log(requestAsync.Error);
            return requestAsync.Response;
        }

        public async Task<GetMarketSellItemsResponse> GetSellRequest(int skip, int getItems)
        {
            var sellRequest = new GetMarketSellItemsRequest() { Skip = skip, Take = getItems };

            var requestAsync = await _netModule.CustomNetwork.SendRequestAsync(
                operations => operations.GetMarketSellItemsOp, sellRequest);
            if (!requestAsync.Success)
                Debug.Log(requestAsync.Error);
            return requestAsync.Response;
        }

        private async Task<GetMarketLotItemsResponse> GetLotsRequest(int skip, int getItems)
        {
            var lotsRequest = new GetMarketLotItemsRequest() { Skip = skip, Take = getItems };
            var requestAsync = await _netModule.CustomNetwork.SendRequestAsync(
                operations => operations.GetMarketLotItemsOp, lotsRequest
            );
            if (!requestAsync.Success)
                Debug.Log(requestAsync.Error);
            return requestAsync.Response;
        }

        public void Dispose()
        {
            /*	if(_billingController != null)
                    _billingController.OnBuyItemLock -= BuyItemLock;*/
        }
    }

    public struct ItemsResult
    {
        public List<ConstructionItemData> constructionItemData;
        public int totalItemsCount;
        public int skip;
    }
}
