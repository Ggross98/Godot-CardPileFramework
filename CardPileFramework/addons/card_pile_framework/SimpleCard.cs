namespace Ggross.CardPileFramework;

using Ggross.CardPileFramework;
using Godot;
using System;

/// <summary>
/// Implementation of a simple card UI object considering draw, discard and hand piles
/// </summary>
public partial class SimpleCard : Card
{
    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();
        if(IsInteractive()) {
            ((SimpleCardPileManager)manager).UpdateCardsZIndex();
        }
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();
        if (!IsClicked && IsMouseHovering){
            ((SimpleCardPileManager)manager).UpdateCardsZIndex();
        }
    }

    protected override void OnGuiInput(InputEvent @event)
    {
        base.OnGuiInput(@event);

        var m = (SimpleCardPileManager)manager;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left){
            if (mouseEvent.Pressed){

                // The dragged card will be shown at the front. 
                if(IsInteractive()){
                    m.UpdateCardsZIndex();
                }

                // Click the draw pile to draw a card.
                if (m.GetCardPileSize(SimpleCardPileManager.DropzoneType.DrawPile) > 0 && 
                        m.IsPileEnabled(SimpleCardPileManager.DropzoneType.HandPile) &&
                        m.GetCardsInPile(SimpleCardPileManager.DropzoneType.DrawPile).Contains(this) && 
                        !m.IsAnyCardClicked() && 
                        m.clickDrawPileToDraw)
                {
                    m.DrawCard(1);
                }

            }
            else{
                if (m.IsCardInHand(this))
                {
                    m.UpdateCardsTargetPosition();
                    m.UpdateCardsZIndex();
                }
            }
        }
    }

}
