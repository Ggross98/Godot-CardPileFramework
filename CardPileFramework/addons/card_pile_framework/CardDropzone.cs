namespace Ggross.CardPileFramework;

using Godot;
using Godot.Collections;

public partial class CardDropzone : Control
{
    [Export]
    public CardManager Manager { get; set; }

    // [Export] public bool enabled = true;
    public bool IsMouseHovering { get; protected set; }

    public enum DropzoneCardLayout
    {
        Up,
        Left,
        Right,
        Down,
    }

    [Export]
    public DropzoneCardLayout layout = DropzoneCardLayout.Up;

    // [Export] public DropzoneType pilesType = DropzoneType.Dropzone;

    protected Array<Card> _holdingCards = new Array<Card>();

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public override void _Process(double delta)
    {
        // Check mouse event
        var tmp = GetGlobalRect().HasPoint(GetGlobalMousePosition());
        if (IsMouseHovering && !tmp)
        {
            EmitSignal(SignalName.MouseExited);
        }
        if (!IsMouseHovering && tmp)
        {
            EmitSignal(SignalName.MouseEntered);
        }
        IsMouseHovering = tmp;
    }

    protected virtual void OnMouseEntered()
    {
        GD.Print("Mouse Entered Dropzone: ", Name);
    }

    protected virtual void OnMouseExited()
    {
        GD.Print("Mouse Exited Dropzone: ", Name);
    }

    /// <summary>
    /// Drop the card on the zone. Not added to the zone!
    /// </summary>
    /// <param name="cardUi"></param>
    public void DropCard(Card cardUi)
    {
        OnCardDropped(cardUi);
    }

    protected virtual void OnCardDropped(Card cardUi)
    {
        GD.Print("Card: ", cardUi.Name, "Dropped on Dropzone:", Name);
    }

    public virtual bool CanDropCard(Card cardUi)
    {
        return IsInteractive();
    }

    public virtual bool IsInteractive()
    {
        return Visible;
    }

    public virtual bool IsCardInteractive(Card card)
    {
        if (IsHolding(card))
        {
            return true;
        }
        else
        {
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

    public bool IsAnyCardClicked()
    {
        foreach (var card in _holdingCards)
        {
            if (card.IsClicked)
            {
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

    /// <summary>
    /// Add the card to the dropzone
    /// </summary>
    /// <param name="cardUi"></param>
    public void AddCard(Card cardUi)
    {
        OnCardAdded(cardUi);
    }

    protected virtual void OnCardAdded(Card cardUi)
    {
        GD.Print("Card: ", cardUi.Name, "Added to Dropzone:", Name);
        _holdingCards.Add(cardUi);
    }

    /// <summary>
    /// Remove the card from the dropzone
    /// </summary>
    /// <param name="cardUi"></param>
    public void RemoveCard(Card cardUi)
    {
        OnCardRemoved(cardUi);
    }

    protected virtual void OnCardRemoved(Card cardUi)
    {
        GD.Print("Card: ", cardUi.Name, "Removed from Dropzone:", Name);
        _holdingCards.Remove(cardUi);
    }

    public virtual void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.MoveToFront();

            var targetPos = Position;
            cardUi.TargetPosition = targetPos;
            if (instantlyMove)
            {
                cardUi.Position = targetPos;
            }
        }
    }

    public virtual void UpdateCardsZIndex()
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var cardUi = _holdingCards[i];
            cardUi.ZIndex = cardUi.IsClicked ? 3000 + i : i;
        }
    }
}
