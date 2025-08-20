using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Protocol.DataTransferObjects;
using LoadedLions.ConstructionModule;
using LoadedLions.GlobalModule;
using LoadedLions.GlobalModule.BuildingCardView;
using LoadedLions.Infrastructure;
using LoadedLions.MarketModule.Submodules.MarketPanelModule;
using Stepico.Net.GameApi.Protocol;
using Unity.VisualScripting;
using UnityEngine;

namespace LoadedLions.MarketModule.MarketPanelModule
{
	public interface IMarketPanelFactory : IFactory
	{
		public void Init(RectTransform parrent);
		public Task<MarketPanelView> Create();
	}

	public class MarketPanelFactory : IMarketPanelFactory
	{
		private const string _key = "MarketPanel";

		private readonly IAssetProvider _assetProvider;
		private RectTransform _parrent;
		private IGlobalFactory _globalFactory;
		private IMarketPanelApiHelper _marketPanelApiHelper;
		private IRarityService _rarityService;
		private MarketPanelView _view;
		private ModulesAssets _assets;
		public MarketPanelFactory(IAssetProvider assetProvider,IGlobalFactory globalFactory,IMarketPanelApiHelper marketPanelApiHelper,IRarityService rarityService,ModulesAssets assets)
		{
			_assetProvider = assetProvider;
			_globalFactory = globalFactory;
			_marketPanelApiHelper = marketPanelApiHelper;
			_rarityService= rarityService;
			_assets = assets;
		}
		public void Init(RectTransform parrent)
		{
			_parrent = parrent;
            if (_rarityService == null)
            {
                _rarityService = new RarityService(ScriptableObject.CreateInstance<RaritiesData>());
            }


        }

        public Task<(bool, IEnumerable<ErrorContainer>)> Prepare() =>
            Task.FromResult<(bool, IEnumerable<ErrorContainer>)>((true, Array.Empty<ErrorContainer>()));

        public async Task<MarketPanelView> Create()
		{
            _marketPanelApiHelper.Init(_assetProvider);
			_view = await _assetProvider.Instantiate<MarketPanelView>(_key, parent: (_parrent, false));
			_view.Init(_globalFactory,_rarityService);
			return _view;
		}

        public async Task<ConstructionItemCardView> CreateBuildingCard(Transform container) =>
			await _assetProvider.Instantiate<ConstructionItemCardView>(
				_assets.constructionAssets.constructionItemCardAssetReference,
				parent: (container, false));

        public void Release(GameObject go) =>
			_assetProvider.Release(go);

        public void Release(object obj) =>
			_assetProvider.Release(obj);

        public void Dispose()
        { }
    }
}
