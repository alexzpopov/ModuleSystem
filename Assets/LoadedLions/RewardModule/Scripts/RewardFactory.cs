using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoadedLions.GlobalModule;
using LoadedLions.Infrastructure;
using Stepico.Net.GameApi.Protocol;
using UnityEngine;

namespace LoadedLions.RewardModule
{
	public interface IRewardFactory : IFactory
	{
		public void Init(RectTransform parrent);
		public Task<RewardView> Create();
	}

	public class RewardFactory : IRewardFactory
	{
		private readonly IAssetProvider _assetProvider;
		private readonly IGlobalFactory _globalFactory;
		private readonly RewardAssets _assets;

		private RectTransform _parrent;

		public RewardFactory(
			IAssetProvider assetProvider,
			IGlobalFactory globalFactory,
			RewardAssets assets
			)
		{
			_assetProvider = assetProvider;
			_globalFactory = globalFactory;
			_assets = assets;
		}

		public void Init(RectTransform parrent)
		{
			_parrent = parrent;
		}

		public async Task<(bool successful, IEnumerable<ErrorContainer> errors)> Prepare()
		{
			await _assetProvider.LoadAsset<RewardView>(_assets.rewardViewAssetReference);
			return (true, Array.Empty<ErrorContainer>());
		}

		public async Task<RewardView> Create()
		{
			var view = await _assetProvider.Instantiate<RewardView>(
				_assets.rewardViewAssetReference,
				parent: (_parrent, false));
			view.Init(_globalFactory);
			return view;
		}

		public void Release(GameObject go) =>
			_assetProvider.Release(go);

		public void Release(object obj) =>
			_assetProvider.Release(obj);

        public void Dispose()
        { }
	}
}
