using System;
using System.Globalization;
using System.Text.RegularExpressions;
using LoadedLions.ConstructionModule;
using LoadedLions.GlobalModule;
using LoadedLions.GlobalModule.BuildingCardView;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LoadedLions.MarketModule
{
    public class MarketInfoCard : ItemCardViewBase<ConstructionItemData>
    {
        [SerializeField] protected TMP_Text _goldGenerationRate;
        [SerializeField] protected TMP_Text _IDText;
        [SerializeField] protected Image _rarityQuality;
        [SerializeField] protected TMP_Text _timeCreated;
        [SerializeField] protected TMP_Text _level;
        [SerializeField] protected TMP_Text _cost;
        [SerializeField] protected TMP_InputField _newCost;
        [SerializeField] protected Button close;
        [SerializeField] protected GameObject root;
        [SerializeField] protected GameObject[] _hideBlueprints;

        private ConstructionItemData _data;
        private TimeSpan _serverTimeSecondShift;
        private DateTime _epochStart;
        public event Action OnClose;
        public override ConstructionItemData Data => _data;


        private void OnEnable()
        {
            if (_newCost)
            {
                _newCost.text = "0";
            }
        }
        private new void Start()
        {
            _epochStart = new System.DateTime(2000, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            base.Start();
            if (close != null)
            {
                close.onClick.AddListener(()=>Show(false));
            }
            if (_newCost != null)
            {
                _newCost.onValueChanged.AddListener(OnSetNewCost);
                _newCost.caretWidth = 3;
            }
        }
        private void VerifyInputDigital(string text)
        {
            _newCost.text = Regex.Replace(text,
                @"[-+]", "");

        }
        private void OnSetNewCost(string text)
        {
            VerifyInputDigital(text);
            int resCost=-1;
            NumberStyles styles = NumberStyles.Number;
            var cultureInfo = new CultureInfo("en-US");
            int.TryParse(text,styles,cultureInfo, out resCost);
            _data.Cost =resCost ;
        }

        public void SetTime(TimeSpan serverTimeSecondShift)
        {
            _serverTimeSecondShift = serverTimeSecondShift;
        }
        public void SetData(ConstructionItemData data)
        {
            _data = data;
        }

        public virtual void Setup(string cardName = null,
            Sprite cardSprite = null,
            bool? isSelected = null,
            ItemRarity? cardRarity = null,
            bool? idImageActive = null,
            Sprite idSprite = null,
            string idText = null,
            ItemRarity? idRarity = null,
            double goldGenerationRate = -1,
            int timeCreated=-1,
            int level=-1,
            long cost=-1
            )
        {
            base.Setup(cardName, cardSprite, isSelected);
            if (cardRarity != null)
                SetRarityColor((ItemRarity)cardRarity);
            if (idImageActive != null)
                ItemIdView.SetImageActive((bool)idImageActive);
            if (idSprite != null)
                ItemIdView.SetImage(idSprite);
            if (idText != null)
                ItemIdView.SetText(idText);
            if (idRarity != null)
                ItemIdView.SetRarityColor((ItemRarity)idRarity);

            _goldGenerationRate.text = String.Format("{0}/s",((decimal)goldGenerationRate).PringBig());
            _IDText.text = "#" + _data.Id.ToString();
            _rarityQuality.color= RarityService.GetColor((ItemRarity)cardRarity);

                //var pDate = (DateTime.Now-(new DateTime(2000, 1, 1, 0, 0, 0, 0)).AddSeconds(timeCreated)-_serverTimeSecondShift);
                var pDate = (timeCreated==-1) ? new TimeSpan(): DateTime.Now-_epochStart.AddSeconds(timeCreated) - _serverTimeSecondShift ;
            _timeCreated.text =string.Format("{0:00} H {1:00} M", pDate.Hours,pDate.Minutes);
            _level.text = level.ToString();

            _cost.text =cost<0 ? "" : cost.ToString();
            bool isBusiness = _data.Type == ConstructionItemData.CardType.Business;
            {
                foreach (var item in _hideBlueprints)
                {
                   item.SetActive(isBusiness);
                }
            }
        }

        public void SetRarityColor(ItemRarity rarity) =>
            _cardRarityLightImage.color = RarityService.GetColor(rarity);

        public void ShowInfo(bool status)
        {
            if (this.gameObject)
            {
                this.gameObject.SetActive(status);
            }
        }
        public void Show(bool status)
        {
            root?.gameObject.SetActive(status);
            if (!status)
            {
                OnClose?.Invoke();
            }
        }
    }
}
