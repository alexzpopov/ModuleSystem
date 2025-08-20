using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Base.Types.Enums;
using Common.Base.Types.Interfaces;
using Common.Protocol.DataTransferObjects;
using LoadedLions.CheatsModule;
using LoadedLions.NetModule;

namespace LoadedLions.RewardModule
{
    public interface IRewardModule : IDisposable
    {
        event Action Showed;
        event Action Hided;
        event Action CloseClick;

        void Init();
        Task Show(int level, BlueprintType blueprintType);
        void Hide();
        IEvents Events();
        RewardView GetView();
    }

    public class RewardModule : IRewardModule
    {
        public  event Action Showed;
        public event Action Hided;
        public event Action CloseClick;

        private readonly IPostProcessHandler _postProcessHandler;
        private readonly IRewardFactory _factory;
        public static RewardView _view;

        private BlueprintType _blueprintType = BlueprintType.Unknown;
        private int _level = 0;
        private bool debug = true;
        public IEvents Events => _events;
        private readonly ModuleEvents _events;
        public RewardModule(
            IPostProcessHandler postProcessHandler,
            IRewardFactory factory,
            IEvents events
        )
        {
            _postProcessHandler = postProcessHandler;
            _factory = factory;
            _events = events as ModuleEvents;
        }

        public void Init()
        {
            _postProcessHandler.ResourcesChangedEvent += OnResourcesChangedEvent;
            _postProcessHandler.UniqueItemsChanged += OnUniqueItemsChanged;
        }

        public async Task Show(int level, BlueprintType blueprintType)
        {
            if (_view == null)
            {
                _view = await _factory.Create();
                _view.Hide();
                _view.CloseButtonClick += OnCloseClick;
            }

            await _view.Show(level, blueprintType);
            _view.SetInteractable(true);
            Showed?.Invoke();
        }

        public void Hide()
        {
            _view.Hide();
            Hided?.Invoke();
        }

        IEvents IRewardModule.Events() => _events;
        public RewardView GetView() => _view;

        private void OnCloseClick()
        {
            CloseClick?.Invoke();
            Hide();
        }

        private void OnResourcesChangedEvent(object sender, IEnumerable<ResourcesItemDTO> items)
        {
            foreach (var item in items)
            {
                if (item.StorageItemId.ItemType == ItemType.Personal)
                {
                    if (item.StorageItemId.ItemId == (short)PersonalItemType.Level)
                    {
                        _level = (int)item.Value;
                        CheckForShow();
                        break;
                    }
                }
            }
        }

        private void OnUniqueItemsChanged(object sender, IEnumerable<IUniqueItemDTO> items)
        {
            foreach (var item in items)
            {
                if (item.StorageItemId.ItemType == ItemType.Blueprint)
                {
                    _blueprintType = (BlueprintType)item.StorageItemId.ItemId;
                    CheckForShow();
                    break;
                }
            }
        }

        private async void CheckForShow()
        {
            if (_level > 0 && _blueprintType != BlueprintType.Unknown)
            {
                await Show(_level, _blueprintType);
                _level = 0;
                _blueprintType = BlueprintType.Unknown;
            }
        }

        public void Dispose()
        {
            _postProcessHandler.ResourcesChangedEvent -= OnResourcesChangedEvent;
            _postProcessHandler.UniqueItemsChanged -= OnUniqueItemsChanged;
            if (_view != null)
            {
                _view.CloseButtonClick -= OnCloseClick;
                _factory.Release(_view.gameObject);
            }
        }
    }
}
