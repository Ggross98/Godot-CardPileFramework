namespace Ggross.CardPileFramework;

using System.ComponentModel;
using Godot;
using Godot.Collections;

public partial class CardDropzone : Control
{
    [Export]
    public CardPileManager cardPileManager;
    
    // [Export] public bool heldCardDirection = true;
    public bool mouseIsHovering = false;
    [Export] public bool enabled = true;

    public enum DropzoneType
    {
        DrawPile,
        HandPile,
        DiscardPile,
        Dropzone
    }

    public enum DropzoneCardLayout
    {
        Up,
        Left,
        Right,
        Down
    }

    [Export]
    public DropzoneCardLayout layout = DropzoneCardLayout.Up;

    [Export]
    public DropzoneType pilesType = DropzoneType.Dropzone;

    protected Array<Card> _holdingCards = new Array<Card>();


    public override void _Ready(){
        // _heldCards = new Array<Card>();

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _Process(double delta)
    {
        // UpdateCardsTargetPositions();

        // Check mouse event
        var tmp = GetGlobalRect().HasPoint(GetGlobalMousePosition());
        if(mouseIsHovering && !tmp){
            EmitSignal(SignalName.MouseExited);
        }
        if(!mouseIsHovering && tmp){
            EmitSignal(SignalName.MouseEntered);
        }
        mouseIsHovering = tmp;
    }

    public virtual void OnMouseEntered(){

    }

    public virtual void OnMouseExited(){

    }


    public virtual void OnCardDropped(Card cardUi)
    {
        // if (cardPileManager != null)
        // {
        //     cardPileManager.SetCardDropzone(cardUi, this);
        // }
        // else return;
    }

    public virtual bool CanDropCard(Card cardUi)
    {
        return Visible;
    }

    public Card GetTopCard()
    {
        if (_holdingCards.Count > 0)
        {
            return _holdingCards[0];
        }
        return null;
    }

    public Card GetCardAt(int index)
    {
        if (_holdingCards.Count > index)
        {
            return _holdingCards[index];
        }
        return null;
    }

    public int GetTotalHoldingCards()
    {
        return _holdingCards.Count;
    }

    public bool IsHolding(Card cardUi)
    {
        return _holdingCards.Contains(cardUi);
    }

    public bool IsAnyCardClicked(){
        foreach(var card in _holdingCards){
            if(card.isClicked){
                return true;
            }
        }
        return false;
    }

    public Array<Card> GetHoldingCards()
    {
        return new Array<Card>(_holdingCards); // duplicate to allow the user to mess with the array without messing with this one!!!
    }

    public void AddCard(Card cardUi)
    {
        _holdingCards.Add(cardUi);
        // UpdateTargetPositions();
    }

    public void RemoveCard(Card cardUi)
    {
        _holdingCards.Remove(cardUi);
        // UpdateTargetPositions();
    }

    public virtual void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.MoveToFront();

            var targetPos = Position;
            cardUi.targetPosition = targetPos;
            if(instantlyMove){
                cardUi.Position = targetPos;
            }
        }
    }

    public virtual void UpdateCardsZIndex(){
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.ZIndex = cardUi.isClicked ? 3000 + i : i;
        }
    }

}