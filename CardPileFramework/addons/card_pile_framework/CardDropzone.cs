namespace Ggross.CardPileFramework;

using System.ComponentModel;
using Godot;
using Godot.Collections;

public partial class CardDropzone : Control
{
    [Export] public CardPileManager manager;
    // [Export] public bool enabled = true;
    public bool IsMouseHovering { get; protected set;}

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
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _Process(double delta)
    {
        
        // Check mouse event
        var tmp = GetGlobalRect().HasPoint(GetGlobalMousePosition());
        if(IsMouseHovering && !tmp){
            EmitSignal(SignalName.MouseExited);
        }
        if(!IsMouseHovering && tmp){
            EmitSignal(SignalName.MouseEntered);
        }
        IsMouseHovering = tmp;

    }

    protected virtual void OnMouseEntered(){
        GD.Print("Mouse Entered: ", Name);
    }

    protected virtual void OnMouseExited(){

    }


    public virtual void DropCard(Card cardUi)
    {

    }

    public virtual bool CanDropCard(Card cardUi)
    {
        return IsInteractive();
    }

    public virtual bool IsInteractive(){
        return Visible;
    }

    public virtual bool IsCardInteractive(Card card){
        if(IsHolding(card)){
            return true;
        }
        else{
            return false;
        }
    }

    public Card GetTopCard()
    {
        if (_holdingCards.Count > 0)
        {
            return _holdingCards[_holdingCards.Count - 1];
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

    public int CardsCount()
    {
        return _holdingCards.Count;
    }

    public bool IsHolding(Card cardUi)
    {
        return _holdingCards.Contains(cardUi);
    }

    public bool IsAnyCardClicked(){
        foreach(var card in _holdingCards){
            if(card.IsClicked){
                return true;
            }
        }
        return false;
    }

    public Array<Card> GetCards()
    {
        // Get a copy of holding cards array. 
        return [.. _holdingCards]; 
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
            cardUi.TargetPosition = targetPos;
            if(instantlyMove){
                cardUi.Position = targetPos;
            }
        }
    }

    public virtual void UpdateCardsZIndex(){
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.ZIndex = cardUi.IsClicked ? 3000 + i : i;
        }
    }

}