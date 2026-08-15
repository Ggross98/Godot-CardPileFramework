namespace Ggross.CardPileFramework;

using Godot;

[GlobalClass]
public partial class CardStackPile : CardPile
{
    public enum StackLayout
    {
        Up,
        Left,
        Right,
        Down,
    }

    [Export]
    public StackLayout layout = StackLayout.Up;

    [Export]
    public int stackDisplayGap = 8;

    [Export]
    public int maxStackDisplay = 6;

    [Export]
    public bool cardUIFaceUp = true;

    public CardStackPile()
    {
        OnlyTopCardInteractive = true;
    }

    public override void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var card = _holdingCards[i];
            if (card.IsClicked)
                continue;

            var targetPos = Position;
            var offset = i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
            switch (layout)
            {
                case StackLayout.Up:
                    targetPos.Y -= offset;
                    break;
                case StackLayout.Down:
                    targetPos.Y += offset;
                    break;
                case StackLayout.Right:
                    targetPos.X += offset;
                    break;
                case StackLayout.Left:
                    targetPos.X -= offset;
                    break;
            }

            card.SetDirection(cardUIFaceUp ? Vector2.Up : Vector2.Down);
            card.Rotation = 0;
            card.MoveToFront();
            card.TargetPosition = targetPos;
            if (instantlyMove)
                card.Position = targetPos;
        }
    }
}
