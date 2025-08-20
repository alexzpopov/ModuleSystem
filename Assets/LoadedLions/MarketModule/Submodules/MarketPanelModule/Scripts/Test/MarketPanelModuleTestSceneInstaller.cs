using LoadedLions.GlobalModule;
using LoadedLions.Infrastructure;
using Stepico.IOC;
using UnityEngine;

namespace LoadedLions.MarketModule.MarketPanelModule
{
	public class MarketPanelModuleTestSceneInstaller : SceneInstaller
	{
		[Header("Dependencies")]
		[SerializeField] private RectTransform _uiRootTransform;

		protected override void SceneInstall(ContextBuilder contextBuilder)
		{
			contextBuilder.Register<MarketPanelModuleTestState>().AsSingleton();
			contextBuilder.Register<MarketPanelFactory>().As<IMarketPanelFactory>().AsScoped();
			contextBuilder.Register<MarketPanelModule>().As<IMarketPanelModule>().AsScoped();
			contextBuilder.Register<MarketPanelApiHelper>().As<IMarketPanelApiHelper>().AsScoped();
		}

		protected override void OnSceneContextBuilt(IContext context)
		{
			var stateMachine = context.Resolve<IStateMachine>();
			stateMachine.AddState(context.Resolve<MarketPanelModuleTestState>());
			context.Resolve<IMarketPanelFactory>().Init(_uiRootTransform);
		}

		protected override void BeforeSceneContextDispose(IContext context) { }
	}
}
