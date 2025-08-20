using LoadedLions.GlobalModule;
using LoadedLions.Infrastructure;
using Stepico.IOC;

namespace LoadedLions.RewardModule
{
	public class RewardModuleTestSceneInstaller : LandSceneInstaller
	{
		protected override void SceneInstall(ContextBuilder contextBuilder)
		{
			base.SceneInstall(contextBuilder);
			contextBuilder.Register<RewardModuleTestState>().AsSingleton();
		}

		protected override void OnSceneContextBuilt(IContext context)
		{
			base.OnSceneContextBuilt(context);
			var stateMachine = context.Resolve<IStateMachine>();
			var state = context.Resolve<RewardModuleTestState>();
			stateMachine.AddState(state);
		}
	}
}