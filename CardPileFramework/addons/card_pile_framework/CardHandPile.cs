namespace Ggross.CardPileFramework;

using Godot;

[GlobalClass]
public partial class CardHandPile : CardPile
{
    [Export]
    public int MaxHandSize { get; set; }

    [Export]
    public int MaxHandSpread { get; set; }

    [Export]
    protected Curve handRotationCurve,
        handVerticalCurve;

    [Export]
    protected bool handFaceUp = true;

    public bool IsFull() => MaxHandSize > 0 && _holdingCards.Count >= MaxHandSize;

    public override void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var card = _holdingCards[i];
            if (card.IsClicked)
                continue;

            card.MoveToFront();
            var handRatio = _holdingCards.Count > 1 ? (float)i / (_holdingCards.Count - 1) : 0.5f;

            var targetPos = Position + new Vector2(Size.X / 2, 0);
            var cardSpacing = MaxHandSpread / (float)(_holdingCards.Count + 1);
            targetPos.X += (i + 1) * cardSpacing - MaxHandSpread / 2.0f;
            if (handVerticalCurve != null)
                targetPos.Y -= handVerticalCurve.SampleBaked(handRatio);
            if (handRotationCurve != null)
                card.Rotation = Mathf.DegToRad(handRotationCurve.SampleBaked(handRatio));
            else
                card.Rotation = 0;

            if (card.IsMouseHovering)
                targetPos.Y -= card.HoverDistance;

            card.SetDirection(handFaceUp ? Vector2.Up : Vector2.Down);
            card.TargetPosition = targetPos;
            if (instantlyMove)
                card.Position = targetPos;
        }
    }

    public override void UpdateCardsZIndex()
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var card = _holdingCards[i];
            card.ZIndex = 1000 + i;
            card.MoveToFront();
            if (card.IsMouseHovering)
                card.ZIndex = 2000 + i;
            if (card.IsClicked)
                card.ZIndex = 3000 + i;
        }
    }
}
