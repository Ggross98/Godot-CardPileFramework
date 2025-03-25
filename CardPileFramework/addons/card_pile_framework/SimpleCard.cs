namespace Ggross.CardPileFramework;

using Ggross.CardPileFramework;
using Godot;
using System;

/// <summary>
/// Implementation of a simple card UI object considering draw, discard and hand piles
/// </summary>
public partial class SimpleCard : Card
{
    protected SimpleCardPileManager manager;

    public override void _Ready()
    {
        base._Ready();

        manager = GetParent<SimpleCardPileManager>();
        
    }

    protected override void SetDisabled(bool val)
    {
        base.SetDisabled(val);

        if (val)
        {
            manager.ResetCardsZIndex();
        }
    }

    protected override bool IsInteractable()
    {
        var valid = false;

        // In hand pile
        if (manager.IsCardInHand(this))
        {
            valid = manager.handEnabled && !manager.IsAnyCardUiClicked();
        }
        // In any dropzone
        else{
            var dropzone = manager.GetCardDropzone(this);
            if (dropzone != null)
            {
                valid = dropzone.GetTopCard() == this && !manager.IsAnyCardUiClicked();
            }
        }

        return valid;
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();
        if(IsInteractable()) {
            manager.ResetCardsZIndex();
        }
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();
        if (!isClicked && mouseIsHovering){
            manager.ResetCardsZIndex();
        }
    }

    protected override void OnGuiInput(InputEvent @event)
    {
        base.OnGuiInput(@event);

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left){
            if (mouseEvent.Pressed){

                // The dragged card will be shown at the front. 
                if(IsInteractable()){
                    manager.ResetCardsZIndex();
                }

                // Click the draw pile to draw a card.
                if (manager.GetCardPileSize(SimpleCardPileManager.PilesType.DrawPile) > 0 && 
                        manager.handEnabled &&
                        manager.GetCardsInPile(SimpleCardPileManager.PilesType.DrawPile).Contains(this) && 
                        !manager.IsAnyCardUiClicked() && 
                        manager.clickDrawPileToDraw)
                {
                    manager.Draw(1);
                }

            }
            else{
                if (manager.IsCardInHand(this))
                {
                    manager.CallDeferred("ResetTargetPositions");
                }
            }
        }
    }

}
