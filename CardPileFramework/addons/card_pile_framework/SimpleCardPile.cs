using Ggross.CardPileFramework;
using Godot;

public partial class SimpleCardPile : CardDropzone
{
    [Export]
    public int stackDisplayGap = 8;
    [Export]
    public int maxStackDisplay = 6;
    [Export]
    public bool cardUIFaceUp = true;
    [Export]
    public bool canDragTopCard = true;

    protected override void OnCardDropped(Card cardUi)
    {
        if (manager != null)
        {
            manager.SetCardDropzone(cardUi, this);
        }
    }

    public override void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            var targetPos = Position;
            switch (layout)
            {
                case DropzoneCardLayout.Up:
                    targetPos.Y -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
                case DropzoneCardLayout.Down:
                    targetPos.Y += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
                case DropzoneCardLayout.Right:
                    targetPos.X += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
                case DropzoneCardLayout.Left:
                    targetPos.X -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
            }
            cardUi.SetDirection(cardUIFaceUp ? Vector2.Up : Vector2.Down);
            cardUi.Rotation = 0;
            cardUi.MoveToFront(); // must also do this to account for INVISIBLE INTERACTION ORDER
            cardUi.TargetPosition = targetPos;
            if(instantlyMove) cardUi.Position = targetPos;
        }
    }

    public override bool IsCardInteractive(Card card)
    {
        if(base.IsCardInteractive(card)){
            return !manager.IsAnyCardClicked() && canDragTopCard && card == GetTopCard();
        }
        return false;
    }
}
