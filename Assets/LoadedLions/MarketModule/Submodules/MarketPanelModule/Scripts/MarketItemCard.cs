using LoadedLions.ConstructionModule;
using LoadedLions.ConstructionModule.ConstructionItemsMenuModule;
using LoadedLions.GlobalModule;
using UnityEngine;

namespace LoadedLions.MarketModule
{
    public class MarketItemCard : ItemCardViewBase<ConstructionItemData>
    {
        private ConstructionItemData _data;
        public override ConstructionItemData Data => _data;
        private ConstructionItemsMenuConfig _config;
        private new void Start()
        {
            base.Start();
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
            int level=-1,
            long cost=-1
            )
        {
            if (_config==null)
                _config= Resources.Load<ConstructionItemsMenuConfig>(nameof(ConstructionItemsMenuConfig));

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

            idSprite= _config.CroIcon;
            ItemIdView.SetImage(idSprite);

            SetCost(cost);
            SetLevel(level);

        }

        public void SetRarityColor(ItemRarity rarity)
        {
            if ((_cardRarityLightImage!=null)&&(RarityService!=null))
            {
                    _cardRarityLightImage.color = RarityService.GetColor(rarity);
            }
        }
    }
}
