using System;
using System.Threading.Tasks;
using Common.Base.Types.Enums;
using LoadedLions.GlobalModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadedLions.RewardModule
{
	public class RewardView : MonoBehaviour
	{
		private const string _blueprintKey = "Blueprint_";

        public event Action OnShow;
		public event Action CloseButtonClick;

		[SerializeField] private TMP_Text _levelText;
		[SerializeField] private Image _rewardImage;
		[SerializeField] private Button _closeButton;
		[SerializeField] private CanvasGroup _canvasGroup;
		private IGlobalFactory _globalFactory;
		private void Start()
		{
			_closeButton.onClick.AddListener(() => CloseButtonClick?.Invoke());
		}

		public void Init(IGlobalFactory globalFactory)
		{
			_globalFactory = globalFactory;
		}

		public async Task Show(int level, BlueprintType blueprintType)
		{
			_levelText.text = level.ToString();
			SetRewardImage(await _globalFactory.GetSpriteByAssetPath(_blueprintKey + blueprintType));
            transform.SetAsLastSibling();
			gameObject.SetActive(true);
            OnShow?.Invoke();
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}

		public void SetInteractable(bool value) =>
			_canvasGroup.interactable = value;

		private void SetRewardImage(Sprite sprite) =>
			_rewardImage.sprite = sprite;

		private void OnDestroy()
		{
			_closeButton.onClick.RemoveAllListeners();
		}
	}
}

