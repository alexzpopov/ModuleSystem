using System;
using LoadedLions.ConstructionModule;
using LoadedLions.MarketModule.MarketPanelModule;

namespace LoadedLions.MarketModule
{
	public interface IMarketModule : IDisposable
    {
        public void Init();
        public void ShowSell(ConstructionItemData cardData,Action OnFinish);

		public IMarketPanelModule MarketPanelModule { get; }

	}
}
