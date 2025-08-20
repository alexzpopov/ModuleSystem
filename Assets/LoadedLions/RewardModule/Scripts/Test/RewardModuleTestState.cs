using Common.Base.Types.Enums;
using LoadedLions.Infrastructure;
using LoadedLions.NetModule;

namespace LoadedLions.RewardModule
{
	public class RewardModuleTestState : IState
	{
		private readonly INetModule _netModule;
		private readonly IRewardModule _rewardModule;

		public RewardModuleTestState(
			INetModule netModule,
			IRewardModule rewardModule
			)
		{
			_netModule = netModule;
			_rewardModule = rewardModule;
		}

		public async void Enter()
		{
			_netModule.Init(Logger.UnityLogger);
			await _netModule.Auth();

			await _rewardModule.Show(5,BlueprintType.Common);
		}

		public void Exit()
		{
			_rewardModule.Hide();
		}
	}
}
