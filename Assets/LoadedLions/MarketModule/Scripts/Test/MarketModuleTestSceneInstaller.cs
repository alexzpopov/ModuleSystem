using LoadedLions.GlobalModule;
using LoadedLions.Infrastructure;
using LoadedLions.MainMenuModule;
using LoadedLions.MarketModule.MarketPanelModule;
using Stepico.IOC;

namespace LoadedLions.MarketModule.Test
{
    public class MarketModuleTestSceneInstaller : LandSceneInstaller
    {
        protected override void SceneInstall(ContextBuilder contextBuilder)
        {
            base.SceneInstall(contextBuilder);
            contextBuilder.ChangeRegister<MainMenuApiHelper, MainMenuMockApiHelper>();

            contextBuilder.Register<MarketModuleTestState>().AsSingleton();
            contextBuilder.Register<MarketModule>().As<IMarketModule>().AsScoped();
            contextBuilder.Register<MarketPanelFactory>().As<IMarketPanelFactory>().AsScoped();
            contextBuilder.Register<MarketPanelModule.MarketPanelModule>().As<IMarketPanelModule>().AsScoped();
            contextBuilder.Register<MarketPanelApiHelper>().As<IMarketPanelApiHelper>().AsScoped();
        }

        protected override void OnSceneContextBuilt(IContext context)
        {
            base.OnSceneContextBuilt(context);
            var stateMachine = context.Resolve<IStateMachine>();
            stateMachine.AddState(context.Resolve<MarketModuleTestState>());
        }

        protected override void BeforeSceneContextDispose(IContext context)
        {
            context.Resolve<IMarketModule>().Dispose();
        }
    }
}
