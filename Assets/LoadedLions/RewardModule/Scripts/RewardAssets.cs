using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoadedLions.RewardModule
{
	[CreateAssetMenu(menuName = "ModulesAssets/RewardAssets", fileName = "RewardAssets")]
	public class RewardAssets : ScriptableObject
	{
		public AssetReference rewardViewAssetReference;
	}
}