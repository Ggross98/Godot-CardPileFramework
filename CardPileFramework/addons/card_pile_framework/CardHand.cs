namespace Ggross.CardPileFramework;
using Godot;
using System;

/// <summary>
/// Hand pile
/// </summary>
public partial class CardHand : CardDropzone
{
    [Export]
    public int maxHandSize = 10;
    [Export]
    public int maxHandSpread = 700;
    [Export]
    public Curve handRotationCurve;
    [Export]
    public Curve handVerticalCurve;
    // [Export]
    // public bool handEnabled = true;
    [Export]
    public bool handFaceUp = true;

    public override void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.MoveToFront();
            var handRatio = _holdingCards.Count > 1 ? (float)i / (_holdingCards.Count - 1) : 0.5f;
            var targetPos = Position;
            var cardSpacing = maxHandSpread / (_holdingCards.Count + 1);
            targetPos.X += (i + 1) * cardSpacing - maxHandSpread / 2.0f;
            if (handVerticalCurve != null)
                targetPos.Y -= handVerticalCurve.SampleBaked(handRatio);
            if (handRotationCurve != null)
                cardUi.Rotation = Mathf.DegToRad(handRotationCurve.SampleBaked(handRatio));
            cardUi.SetDirection(handFaceUp ? Vector2.Up : Vector2.Down);

            cardUi.targetPosition = targetPos;
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
            if (cardUi.mouseIsHovering)
                cardUi.ZIndex = 2000 + i;
            if (cardUi.isClicked)
                cardUi.ZIndex = 3000 + i;
        }
    }

    public bool IsFull(){
        return _holdingCards.Count >= maxHandSize;
    }
}
