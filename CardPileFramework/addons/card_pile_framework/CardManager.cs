namespace Ggross.CardPileFramework;

using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// Creates card UI nodes, owns pile / drop-target registration, and is the only
/// entry point for changing which pile a card belongs to.
/// </summary>
[GlobalClass]
public partial class CardManager : Control
{
    #region Signals
    [Signal]
    public delegate void CardRemovedFromPileEventHandler(CardPile pile, Card card);

    [Signal]
    public delegate void CardAddedToPileEventHandler(CardPile pile, Card card);

    [Signal]
    public delegate void CardHoveredEventHandler(Card card);

    [Signal]
    public delegate void CardUnhoveredEventHandler(Card card);

    [Signal]
    public delegate void CardLeftClickedEventHandler(Card card);

    [Signal]
    public delegate void CardLeftReleasedEventHandler(Card card);

    [Signal]
    public delegate void CardRightClickedEventHandler(Card card);

    [Signal]
    public delegate void CardRightReleasedEventHandler(Card card);

    [Signal]
    public delegate void CardDroppedOnTargetEventHandler(Card card, CardDropTarget target);

    [Signal]
    public delegate void CardRemovedFromGameEventHandler(Card card);
    #endregion

    [Export]
    protected PackedScene cardUIPrefab;

    [Export]
    public Array<CardPile> Piles { get; set; } = new();

    [Export]
    public Array<CardDropTarget> DropTargets { get; set; } = new();

    [ExportGroup("Card Motion")]
    [Export]
    public float CardReturnSpeed { get; set; } = 0.15f;

    [Export]
    public int CardHoverDistance { get; set; } = 30;

    [Export]
    public bool DragWhenClicked { get; set; } = true;

    readonly System.Collections.Generic.Dictionary<Card, CardPile> _cardPiles = new();

    public override void _Ready()
    {
        Piles ??= new Array<CardPile>();
        DropTargets ??= new Array<CardDropTarget>();

        foreach (var pile in Piles)
            BindPile(pile);
        foreach (var target in DropTargets)
            BindDropTarget(target);
    }

    public void RegisterPile(CardPile pile)
    {
        if (pile == null)
            return;
        Piles ??= new Array<CardPile>();
        if (!Piles.Contains(pile))
            Piles.Add(pile);
        BindPile(pile);
    }

    public void RegisterDropTarget(CardDropTarget target)
    {
        if (target == null)
            return;
        DropTargets ??= new Array<CardDropTarget>();
        if (!DropTargets.Contains(target))
            DropTargets.Add(target);
        BindDropTarget(target);
    }

    public Card CreateCard(CardData cardData)
    {
        var card = cardUIPrefab.Instantiate<Card>();
        card.Manager = this;
        card.CardData = cardData;
        card.SetControlParameters(CardReturnSpeed, CardHoverDistance, DragWhenClicked);
        card.Visible = false;

        card.CardHovered += OnCardHovered;
        card.CardUnhovered += OnCardUnhovered;
        card.CardLeftClicked += OnCardLeftClicked;
        card.CardLeftReleased += OnCardLeftReleased;
        card.CardRightClicked += OnCardRightClicked;
        card.CardRightReleased += OnCardRightReleased;

        AddChild(card);
        return card;
    }

    public Card CreateCardInPile(CardData cardData, CardPile pile)
    {
        var card = CreateCard(cardData);
        MoveToPile(card, pile);
        return card;
    }

    public virtual void MoveToPile(Card card, CardPile pile, bool updateLayout = true)
    {
        if (card == null || pile == null)
            return;

        if (_cardPiles.TryGetValue(card, out var current) && current == pile)
            return;

        if (current != null)
        {
            current.RemoveCard(card);
            _cardPiles.Remove(card);
            OnCardRemovedFromPile(current, card);
        }

        pile.AddCard(card);
        _cardPiles[card] = pile;
        card.Visible = true;
        OnCardAddedToPile(pile, card);

        if (updateLayout)
        {
            UpdateCardsTargetPosition();
            UpdateCardsZIndex();
        }
    }

    public virtual void RemoveCardFromGame(Card card)
    {
        if (card == null)
            return;

        if (_cardPiles.TryGetValue(card, out var pile))
        {
            pile.RemoveCard(card);
            _cardPiles.Remove(card);
            OnCardRemovedFromPile(pile, card);
        }

        OnCardRemovedFromGame(card);
        card.QueueFree();

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    public CardPile GetPile(Card card) =>
        card != null && _cardPiles.TryGetValue(card, out var pile) ? pile : null;

    public void HandleCardReleased(Card card)
    {
        if (card == null || DropTargets == null)
        {
            UpdateCardsTargetPosition();
            UpdateCardsZIndex();
            return;
        }

        var mouse = GetGlobalMousePosition();
        var hits = new System.Collections.Generic.List<CardDropTarget>();
        foreach (var target in DropTargets)
        {
            if (target != null && target.Visible && target.ContainsGlobalPoint(mouse))
                hits.Add(target);
        }

        hits.Sort(
            (a, b) =>
            {
                var areaA = a.GetGlobalRect().Size.X * a.GetGlobalRect().Size.Y;
                var areaB = b.GetGlobalRect().Size.X * b.GetGlobalRect().Size.Y;
                return areaA.CompareTo(areaB);
            }
        );

        var pileBefore = GetPile(card);
        foreach (var target in hits)
        {
            target.NotifyDropped(card);
            OnCardDroppedOnTarget(card, target);

            if (!GodotObject.IsInstanceValid(card) || GetPile(card) != pileBefore)
                break;
        }

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    public virtual void UpdateCardsTargetPosition(bool instantlyMove = false)
    {
        if (Piles == null)
            return;
        foreach (var pile in Piles)
            pile?.UpdateCardsTargetPositions(instantlyMove);
    }

    public virtual void UpdateCardsZIndex()
    {
        if (Piles == null)
            return;
        foreach (var pile in Piles)
            pile?.UpdateCardsZIndex();
    }

    public bool IsAnyCardClicked()
    {
        foreach (var child in GetChildren())
        {
            if (child is Card card && card.IsClicked)
                return true;
        }

        return false;
    }

    void BindPile(CardPile pile)
    {
        if (pile != null)
            pile.Manager = this;
    }

    void BindDropTarget(CardDropTarget target)
    {
        if (target != null)
            target.Manager = this;
    }

    protected virtual void OnCardRemovedFromPile(CardPile pile, Card card)
    {
        EmitSignal(SignalName.CardRemovedFromPile, pile, card);
    }

    protected virtual void OnCardAddedToPile(CardPile pile, Card card)
    {
        EmitSignal(SignalName.CardAddedToPile, pile, card);
    }

    protected virtual void OnCardHovered(Card card)
    {
        EmitSignal(SignalName.CardHovered, card);
    }

    protected virtual void OnCardUnhovered(Card card)
    {
        EmitSignal(SignalName.CardUnhovered, card);
    }

    protected virtual void OnCardLeftClicked(Card card)
    {
        EmitSignal(SignalName.CardLeftClicked, card);
    }

    protected virtual void OnCardLeftReleased(Card card)
    {
        EmitSignal(SignalName.CardLeftReleased, card);
    }

    protected virtual void OnCardRightClicked(Card card)
    {
        EmitSignal(SignalName.CardRightClicked, card);
    }

    protected virtual void OnCardRightReleased(Card card)
    {
        EmitSignal(SignalName.CardRightReleased, card);
    }

    protected virtual void OnCardDroppedOnTarget(Card card, CardDropTarget target)
    {
        EmitSignal(SignalName.CardDroppedOnTarget, card, target);
    }

    protected virtual void OnCardRemovedFromGame(Card card)
    {
        EmitSignal(SignalName.CardRemovedFromGame, card);
    }
}
