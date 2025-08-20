using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoadedLions.MarketModule
{
    [ExecuteInEditMode]
    public class MarketPaginatorPoint : MonoBehaviour
    {
        [SerializeField] private Button _pointButton;
        [SerializeField] private GameObject _selectedImage;
        [SerializeField] private bool _selected;
        public TMP_Text text;
        public event Action PointClick;

        private bool _selectedOldValue;

        public void Awake()
        {
            _pointButton.onClick.AddListener(() => PointClick?.Invoke());
        }

        private void OnValidate()
        {
            if (_selected != _selectedOldValue)
            {
                if (_selected)
                    Select();
                else
                    Deselect();
                _selectedOldValue = _selected;
            }
        }

        public void OnDestroy()
        {
            _pointButton.onClick.RemoveAllListeners();
        }

        public void Select() =>
            _selectedImage.SetActive(true);

        public void Deselect() =>
            _selectedImage.SetActive(false);
    }
}
