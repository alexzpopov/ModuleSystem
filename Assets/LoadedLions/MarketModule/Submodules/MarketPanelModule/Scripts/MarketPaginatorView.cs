using System;
using System.Collections.Generic;
using LoadedLions.MainMenuModule;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace LoadedLions.MarketModule
{
    public class MarketPaginatorView : MonoBehaviour
    {
        [SerializeField] private MarketPaginatorPoint _pointPrefab;
        [SerializeField] private Transform _pointsContainer;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;

        [SerializeField, ReadOnly] private int _currentValue;

        public event Action<int> ValueChange;

        private List<MarketPaginatorPoint> _points = new List<MarketPaginatorPoint>();
        [SerializeField] public int _startpoint = 0;
        public int endpoint = 54;
        [SerializeField] private int MAXSELEMENTS = 4;

        #region Debug

        [Range(0, 10)] [SerializeField] private int _amountOfPoints = 6;
        [Range(0, 10)] [SerializeField] private int _selectedIndex;

        [ButtonMethod]
        private void Init() =>
            Init(_amountOfPoints, _selectedIndex, endpoint);

        [ButtonMethod]
        private void Prev()
        {
            int value = ValidatePaginatorValue(CurrentValue - 1);
            if (_points[value].text.text == "...")
            {
                // 0124567_9
                _startpoint -= _amountOfPoints+1;
                if (_startpoint < 0)
                    _startpoint = 0;
            }

            if ((CurrentValue == 0) && (_startpoint > 0))
            {
                _startpoint--;
                UpdatePointsText(_points);
            }
            else
            {
                CurrentValue = value;
            }

            OnValueChange();
        }

        [ButtonMethod]
        private void Next()
        {
            int value = ValidatePaginatorValue(CurrentValue + 1);
            if (CurrentValue == value)
                return;
            if (_points[value].text.text == "...")
            {
                _startpoint += 1;
                UpdatePointsText(_points);
            }
            else
            {
                if (value > endpoint)
                    value = endpoint;
                CurrentValue = value;
            }
            OnValueChange();
        }

        [ButtonMethod]
        private void Click()
        {
            CurrentValue = _selectedIndex;
        }

        #endregion

        public void ResetCurrentValue()
        {
            _currentValue = 0;
        }

        private int CurrentValue
        {
            get => _currentValue;
            set
            {
                if (_currentValue == value)
                {
                    if (_points.Count - 1 >= value)
                    {
                        _points[value].Select();
                    }
                    else
                    {
                        _currentValue = 0;
                    }

                    return;
                }

                if ((_currentValue > 0) && (_points.Count > 0))
                {
                    if (_currentValue < _points.Count)
                    {
                        _points[_currentValue].Deselect();
                    }
                }

                if (_points.Count > 0)
                {
                    if (value >= _points.Count)
                    {
                        value = _points.Count - 1;
                    }
                    _points[value].Select();
                }
                else
                {
                    _currentValue = 0;
                    return;
                }

                if (_points[value].text.text != "...")
                {
                    _currentValue = value;
                    if ((_startpoint + value) >= endpoint)
                        _startpoint--;
                    if (_startpoint < 0)
                        _startpoint = 0;

                }

                // OnValueChange();
            }
        }

        private void OnValueChange()
        {
            if (CurrentValue == _amountOfPoints - 1)
            {
                _startpoint=(endpoint-(_amountOfPoints - 1)-1);
                ValueChange?.Invoke(CurrentValue);
            }
            else
            {
                Debug.Log("PAGE "+CurrentValue+" " + _startpoint+" "+(CurrentValue + _startpoint));
                ValueChange?.Invoke(CurrentValue); //+ _startpoint
            }
        }

        public void Awake()
        {
            _currentValue = -1;
            _previousButton.onClick.AddListener(() => Prev());
            _nextButton.onClick.AddListener(() => Next());
        }

        public void OnDestroy()
        {
            _previousButton.onClick.RemoveAllListeners();
            _nextButton.onClick.RemoveAllListeners();
        }

        public void Init(int amountOfPoints, int selectedIndex, int totalPages)
        {

            endpoint = totalPages;
            DestroyPoints();
            _points = CreatePoints(amountOfPoints);
            if ((selectedIndex - _startpoint) > amountOfPoints)
            {
                _startpoint = selectedIndex - (amountOfPoints - 1);//236-5
                                                                //5
                CurrentValue = amountOfPoints - 1;
            }
            else
            {
                CurrentValue = selectedIndex;// (selectedIndex - _startpoint);
            }

            OnValueChange();
            SetActive(true);
        }

        private void DestroyPoints()
        {
            while (_pointsContainer.childCount > 0)
                DestroyImmediate(_pointsContainer.GetChild(0).gameObject);
        }

        private List<MarketPaginatorPoint> CreatePoints(int amountOfPoints)
        {
            var points = new List<MarketPaginatorPoint>();
            for (int i = 0; i < amountOfPoints; i++)
            {
                var index = i;
                var point = CreatePoint();
                point.PointClick += () =>
                {
                    CurrentValue = index;
                    OnValueChange();
                };
                points.Add(point);
            }

            UpdatePointsText(points);
            return points;
        }

        private void UpdatePointsText(List<MarketPaginatorPoint> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].text.text = (_startpoint + i + 1).ToString();
                points[i].GetComponent<Button>().interactable = true;
            }

            if ((endpoint - _startpoint > MAXSELEMENTS + 1) && (points.Count > MAXSELEMENTS))
            {
                points[points.Count - 2].text.text = "...";
                points[points.Count - 2].GetComponent<Button>().interactable = false;
                points[points.Count - 1].text.text = (endpoint).ToString();
            }
        }

        private MarketPaginatorPoint CreatePoint() =>
            Instantiate(_pointPrefab, _pointsContainer);

        private int ValidatePaginatorValue(int newValue) =>
            newValue < 0 ? 0 :
            newValue >= _points.Count ? _points.Count - 1 : newValue;

        public void SetActive(bool state) =>
            gameObject.SetActive(_points.Count > 1 && state);
    }
}
