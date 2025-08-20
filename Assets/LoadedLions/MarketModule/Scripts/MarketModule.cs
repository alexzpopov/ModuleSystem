using System;
using LoadedLions.ConstructionModule;
using LoadedLions.GlobalModule;
using LoadedLions.MainMenuModule;
using LoadedLions.MarketModule.MarketPanelModule;
using LoadedLions.PopupModule;
using UnityEngine;

namespace LoadedLions.MarketModule
{
    public class MarketModule : IMarketModule
    {
        private readonly IMarketPanelModule _marketPanelModule;
        public IMarketPanelModule MarketPanelModule => _marketPanelModule;
        private readonly IMarketPanelApiHelper _marketPanelApiHelper;
        private readonly IPopupModule _popupModule;
        private readonly IMainMenuModule _mainMenuModule;

        public MarketModule(IMarketPanelModule marketPanelModule, IMarketPanelApiHelper marketPanelApiHelper,
            IMainMenuModule mainMenuModule, IPopupModule popupModule)
        {
            _marketPanelModule = marketPanelModule;
            _marketPanelApiHelper = marketPanelApiHelper;
            _popupModule = popupModule;
            _mainMenuModule = mainMenuModule;
        }

        public async void ShowSell(ConstructionItemData cardData,Action OnFinish)
        {
            await _marketPanelModule.Show(new MarketPanelModel(true));
            _marketPanelModule.ShowSellItem(cardData, OnFinish);
        }

        public void Dispose()
        {
            _mainMenuModule.MarketButtonClick -= Show;
        }

        private async void Show()
        {
            await _marketPanelModule.Show(new MarketPanelModel(false));
        }

        public async void Init()
        {
            _mainMenuModule.MarketButtonClick += Show;
        }

        private void OnResponse(bool state)
        {
            Debug.Log("yes/no " + state);
        }
    }
}
