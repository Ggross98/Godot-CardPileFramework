using Godot;
using System;

public partial class BlockZone : CardDropzone
{
    public override bool CanDropCard(CardUI cardUi)
    {
        if(!base.CanDropCard(cardUi)) return false;
        
        if(cardUi.GetType() == typeof(MyCardUI) && 
            ((MyCardUI)cardUi).cardData.GetType() == typeof(MyCardData))
        {
            var data = (MyCardData)((MyCardUI)cardUi).cardData;
            var cost = data.cost;
            var energy = GetNode<CardBattle>("/root/CardBattle").Energy;

            return (data.type == "Block" || data.type == "Power") && energy >= cost;
        }
        else{
            return false;
        }
    }

    public override void OnCardDropped(CardUI cardUi)
    {
        base.OnCardDropped(cardUi);

        var data = (MyCardData)((MyCardUI)cardUi).cardData;
        GetNode<CardBattle>("/root/CardBattle").Energy -= data.cost;

        if(data.type == "Block"){
            GetNode<CardBattle>("/root/CardBattle").Shield += data.value;
        }

        cardPileUI.SetCardPile(cardUi, CardPileUI.Piles.DiscardPile);
    }
}
