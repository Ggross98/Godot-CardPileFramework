using Ggross.CardPileFramework;
using Godot;

public partial class SkillZone : CardDropTarget
{
    protected override void OnCardDropped(Card card)
    {
        if (card is not MyCard myCard || myCard.CardData is not MyCardData data)
            return;

        var battle = GetNode<CardBattle>("/root/CardBattle");
        if (data.Type != "Skill" || battle.Energy < data.Cost)
            return;

        battle.Energy -= data.Cost;

        if (data.NiceName == "Block")
            battle.Shield += data.Value;

        (Manager as SimpleCardPileManager)?.DiscardCard(card);
    }
}
