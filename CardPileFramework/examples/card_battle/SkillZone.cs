using System;
using Ggross.CardPileFramework;
using Godot;

public partial class SkillZone : CardDropzone
{
    public override bool CanDropCard(Card cardUi)
    {
        if (!base.CanDropCard(cardUi))
            return false;

        if (
            cardUi.GetType() == typeof(MyCard)
            && ((MyCard)cardUi).CardData.GetType() == typeof(MyCardData)
        )
        {
            var data = (MyCardData)((MyCard)cardUi).CardData;
            var cost = data.cost;
            var energy = GetNode<CardBattle>("/root/CardBattle").Energy;

            return data.type == "Skill" && energy >= cost;
        }
        else
        {
            return false;
        }
    }

    protected override void OnCardDropped(Card cardUi)
    {
        var data = (MyCardData)((MyCard)cardUi).CardData;
        GetNode<CardBattle>("/root/CardBattle").Energy -= data.cost;

        // A simple implementation of block card.
        // The potion card is invalid yet.
        if (data.nice_name == "Block")
        {
            GetNode<CardBattle>("/root/CardBattle").Shield += data.value;
        }

        var manager = (SimpleCardPileManager)Manager;
        manager.DiscardCard(cardUi);
    }
}
