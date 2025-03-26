using Ggross.CardPileFramework;
using Godot;
using System;

public partial class SkillZone : CardDropzone
{
    public override bool CanDropCard(Card cardUi)
    {
        if(!base.CanDropCard(cardUi)) return false;
        
        if(cardUi.GetType() == typeof(MyCard) && 
            ((MyCard)cardUi).cardData.GetType() == typeof(MyCardData))
        {
            var data = (MyCardData)((MyCard)cardUi).cardData;
            var cost = data.cost;
            var energy = GetNode<CardBattle>("/root/CardBattle").Energy;

            return data.type == "Skill" && energy >= cost;
        }
        else{
            return false;
        }
    }

    public override void OnCardDropped(Card cardUi)
    {
        base.OnCardDropped(cardUi);

        var data = (MyCardData)((MyCard)cardUi).cardData;
        GetNode<CardBattle>("/root/CardBattle").Energy -= data.cost;

        // A simple implementation of block card.
        // The potion card is invalid yet.
        if(data.nice_name == "Block"){
            GetNode<CardBattle>("/root/CardBattle").Shield += data.value;
        }

        var manager = (MyCardPileManager)cardPileManager;
        manager.Discard(cardUi);
    }
}
