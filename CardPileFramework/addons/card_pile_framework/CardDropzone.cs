namespace Ggross.CardPileFramework;

using Godot;
using Godot.Collections;

public partial class CardDropzone : Control
{
    [Export]
    public SimpleCardPileManager cardPileManager;
    [Export]
    public int stackDisplayGap = 8;
    [Export]
    public int maxStackDisplay = 6;
    [Export]
    public bool cardUIFaceUp = true;
    [Export]
    public bool canDragTopCard = true;
    [Export]
    public bool heldCardDirection = true;
    public bool mouseIsHovering = false;
    [Export]
    public SimpleCardPileManager.PilesCardLayouts layout = SimpleCardPileManager.PilesCardLayouts.Up;

    private Array<Card> _heldCards = new Array<Card>();


    public override void _Ready(){
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _Process(double delta)
    {
        UpdateTargetPositions();

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
        if (cardPileManager != null)
        {
            cardPileManager.SetCardDropzone(cardUi, this);
        }
        else return;
    }

    public virtual bool CanDropCard(Card cardUi)
    {
        return Visible;
    }

    public Card GetTopCard()
    {
        if (_heldCards.Count > 0)
        {
            return _heldCards[_heldCards.Count - 1];
        }
        return null;
    }

    public Card GetCardAt(int index)
    {
        if (_heldCards.Count > index)
        {
            return _heldCards[index];
        }
        return null;
    }

    public int GetTotalHeldCards()
    {
        return _heldCards.Count;
    }

    public bool IsHolding(Card cardUi)
    {
        return _heldCards.Contains(cardUi);
    }

    public Array<Card> GetHeldCards()
    {
        return new Array<Card>(_heldCards); // duplicate to allow the user to mess with the array without messing with this one!!!
    }

    public void AddCard(Card cardUi)
    {
        _heldCards.Add(cardUi);
        // UpdateTargetPositions();
    }

    public void RemoveCard(Card cardUi)
    {
        _heldCards.Remove(cardUi);
        // UpdateTargetPositions();
    }

    private void UpdateTargetPositions()
    {
        for (int i = 0; i < _heldCards.Count; i++)
        {
            var cardUi = _heldCards[i];
            var targetPos = Position;
            switch (layout)
            {
                case SimpleCardPileManager.PilesCardLayouts.Up:
                    targetPos.Y -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
                case SimpleCardPileManager.PilesCardLayouts.Down:
                    targetPos.Y += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
                case SimpleCardPileManager.PilesCardLayouts.Right:
                    targetPos.X += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
                case SimpleCardPileManager.PilesCardLayouts.Left:
                    targetPos.X -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
                    break;
            }
            cardUi.SetDirection(cardUIFaceUp ? Vector2.Up : Vector2.Down);
            cardUi.ZIndex = cardUi.isClicked ? 3000 + i : i;
            cardUi.MoveToFront(); // must also do this to account for INVISIBLE INTERACTION ORDER
            cardUi.targetPosition = targetPos;
        }
    }

}