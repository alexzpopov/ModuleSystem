using LoadedLions.ConstructionModule;
using UnityEngine;

namespace LoadedLions.GlobalModule.BuildingCardView
{
    public class MarketBuildingItemCardView : ItemCardViewBase<ConstructionItemData>
    {
        private ConstructionItemData _data;
        public override ConstructionItemData Data => _data;

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
            string cost = null,
            ItemRarity? idRarity = null)
        {
          //  base.Setup(cardName, cardSprite, isSelected);
            if (cardRarity != null)
                SetRarityColor((ItemRarity)cardRarity);
            if (idImageActive != null)
                ItemIdView.SetImageActive((bool)idImageActive);
            if (idSprite != null)
                ItemIdView.SetImage(idSprite);
            if (cost != null)
                ItemIdView.SetText(cost);
            if (idRarity != null)
                ItemIdView.SetRarityColor((ItemRarity)idRarity);
        }

        public void SetRarityColor(ItemRarity rarity) =>
            _cardRarityLightImage.color = RarityService.GetColor(rarity);
    }
}
