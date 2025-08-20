using LoadedLions.Infrastructure;
using LoadedLions.NetModule;

namespace LoadedLions.MarketModule.MarketPanelModule
{
	public class MarketPanelModuleTestState : IState
	{
		private readonly IMarketPanelModule _marketPanelModule;
		private readonly INetModule _netModule;

		public MarketPanelModuleTestState(
			IMarketPanelModule marketPanelModule,
			INetModule netModule
			)
		{
			_marketPanelModule = marketPanelModule;
			_netModule = netModule;
		}

		public  async void Enter()
		{
            //not used
				_netModule.Init(LoadedLions.Infrastructure.Logger.UnityLogger);
				_netModule.Auth();

			await _marketPanelModule.Show(new MarketPanelModel());
		}
		public void Exit() =>
			_marketPanelModule.Hide();
	}
}
