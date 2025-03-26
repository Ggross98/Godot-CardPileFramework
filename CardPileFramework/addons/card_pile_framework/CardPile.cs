using Ggross.CardPileFramework;
using Godot;
using System;

public partial class CardPile : CardDropzone
{
    [Export]
    public int stackDisplayGap = 8;
    [Export]
    public int maxStackDisplay = 6;
    [Export]
    public bool cardUIFaceUp = true;
    [Export]
    public bool canDragTopCard = true;

    public override void OnCardDropped(Card cardUi)
    {
        if (cardPileManager != null)
        {
            cardPileManager.SetCardDropzone(cardUi, this);
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
            // cardUi.ZIndex = cardUi.isClicked ? 3000 + i : i;
            cardUi.MoveToFront(); // must also do this to account for INVISIBLE INTERACTION ORDER
            cardUi.targetPosition = targetPos;
            if(instantlyMove) cardUi.Position = targetPos;
        }
    }
}
