namespace Ggross.CardPileFramework;
using Godot;
using System;

/// <summary>
/// Hand pile
/// </summary>
public partial class SimpleCardHand : CardDropzone
{
    [Export] public int MaxHandSize {get; protected set;}
    [Export] public int MaxHandSpread {get; protected set;}
    [Export] protected Curve handRotationCurve, handVerticalCurve;
    [Export] protected bool handFaceUp = true;

    public override void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.MoveToFront();
            var handRatio = _holdingCards.Count > 1 ? (float)i / (_holdingCards.Count - 1) : 0.5f;

            var targetPos = Position + new Vector2(Size.X/2, 0);

            var cardSpacing = MaxHandSpread / (_holdingCards.Count + 1);
            targetPos.X += (i + 1) * cardSpacing - MaxHandSpread / 2.0f;
            if (handVerticalCurve != null)
                targetPos.Y -= handVerticalCurve.SampleBaked(handRatio);
            if (handRotationCurve != null)
                cardUi.Rotation = Mathf.DegToRad(handRotationCurve.SampleBaked(handRatio));
            cardUi.SetDirection(handFaceUp ? Vector2.Up : Vector2.Down);

            cardUi.TargetPosition = targetPos;
            if(instantlyMove){
                cardUi.Position = targetPos;
            }
        }
        
    }

    public override void UpdateCardsZIndex()
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.ZIndex = 1000 + i;
            cardUi.MoveToFront();
            if (cardUi.IsMouseHovering)
                cardUi.ZIndex = 2000 + i;
            if (cardUi.IsClicked)
                cardUi.ZIndex = 3000 + i;
        }
    }

    public bool IsFull(){
        return _holdingCards.Count >= MaxHandSize;
    }

    public override bool IsCardInteractive(Card card)
    {
        if(base.IsCardInteractive(card)){
            return !manager.IsAnyCardClicked();
        }
        return false;
    }
}
