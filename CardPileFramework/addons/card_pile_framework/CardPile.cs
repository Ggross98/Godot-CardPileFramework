namespace Ggross.CardPileFramework;

using Godot;
using Godot.Collections;

/// <summary>
/// Ordered card list plus layout. Membership changes only through <see cref="CardManager.MoveToPile"/>.
/// </summary>
[GlobalClass]
public partial class CardPile : Control
{
    public CardManager Manager { get; set; }

    [Export]
    public bool CanDragCards { get; set; } = true;

    [Export]
    public bool OnlyTopCardInteractive { get; set; }

    protected Array<Card> _holdingCards = new();

    public int CardsCount() => _holdingCards.Count;

    public bool IsHolding(Card card) => _holdingCards.Contains(card);

    public Card GetTopCard() =>
        _holdingCards.Count > 0 ? _holdingCards[_holdingCards.Count - 1] : null;

    public Card GetCardAt(int index) =>
        index >= 0 && index < _holdingCards.Count ? _holdingCards[index] : null;

    public Array<Card> GetCards() => [.. _holdingCards];

    public void Shuffle() => _holdingCards.Shuffle();

    public bool IsAnyCardClicked()
    {
        foreach (var card in _holdingCards)
        {
            if (card.IsClicked)
                return true;
        }

        return false;
    }

    public virtual bool IsCardInteractive(Card card)
    {
        if (!Visible || !IsHolding(card) || !CanDragCards)
            return false;
        if (OnlyTopCardInteractive && card != GetTopCard())
            return false;
        if (Manager != null && Manager.IsAnyCardClicked())
            return false;
        return true;
    }

    public void AddCard(Card card)
    {
        if (card == null || _holdingCards.Contains(card))
            return;
        _holdingCards.Add(card);
    }

    public void RemoveCard(Card card)
    {
        _holdingCards.Remove(card);
    }

    public virtual void UpdateCardsTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var card = _holdingCards[i];
            if (card.IsClicked)
                continue;

            card.TargetPosition = Position;
            if (instantlyMove)
                card.Position = card.TargetPosition;
        }
    }

    public virtual void UpdateCardsZIndex()
    {
        for (int i = 0; i < _holdingCards.Count; i++)
        {
            var card = _holdingCards[i];
            card.ZIndex = card.IsClicked ? 3000 + i : i;
        }
    }
}
