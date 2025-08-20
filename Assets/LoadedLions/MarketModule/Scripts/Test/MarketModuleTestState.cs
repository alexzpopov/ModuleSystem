using LoadedLions.GlobalModule;
using LoadedLions.MainMenuModule;
using LoadedLions.NetModule;
using UnityEngine;
using IState = LoadedLions.Infrastructure.IState;
using Logger = LoadedLions.Infrastructure.Logger;

namespace LoadedLions.MarketModule.Test
{
    public class MarketModuleTestState : IState
    {
        private readonly IMarketPanelApiHelper _marketPanelApiHelper;
        private readonly INetModule _netModule;
        private readonly IGlobalApiHelper _globalApiHelper;
        private readonly IMainMenuModule _mainMenuModule;
        private readonly IMarketModule _marketmodule;
        public MarketModuleTestState(
            IMarketPanelApiHelper marketPanelApiHelper,INetModule netModule,IGlobalApiHelper globalApiHelper,IMainMenuModule mainMenuModule,IMarketModule marketmodule)
        {
            _marketPanelApiHelper = marketPanelApiHelper;
            _globalApiHelper = globalApiHelper;
            _netModule = netModule;
            _mainMenuModule = mainMenuModule;
            _marketmodule = marketmodule;
        }

        public void Exit()
        {
        }

        public async void Enter()
        {
            _marketmodule.Init();

            Debug.Log("[MainMenuState] MainMenuModule.Show()");
            _mainMenuModule.Show(MainMenuModule.MainMenuModule.Mode.Land);

            _netModule.Init(Logger.UnityLogger);
            var requestAsync= await _netModule.Auth();
            if (!requestAsync.successful)
            {
                Logger.Log("net Error");
            }
            _netModule.SyncStorageAsync();
            await _globalApiHelper.Prepare();
            //debug
           // _marketPanelApiHelper.SetDebug();
        }
    }
}
