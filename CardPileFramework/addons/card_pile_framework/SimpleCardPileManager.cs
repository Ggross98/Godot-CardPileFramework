namespace Ggross.CardPileFramework;

using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// Optional STS-style wrapper: three named piles and move-only helpers.
/// Does not resolve plays, shuffle discard into draw, or enforce hand size.
/// </summary>
[GlobalClass]
public partial class SimpleCardPileManager : CardManager
{
    public enum PileKind
    {
        Draw,
        Hand,
        Discard,
    }

    [Signal]
    public delegate void DrawPileUpdatedEventHandler();

    [Signal]
    public delegate void HandPileUpdatedEventHandler();

    [Signal]
    public delegate void DiscardPileUpdatedEventHandler();

    [ExportGroup("Piles")]
    [Export]
    protected CardPile drawPile,
        discardPile,
        handPile;

    [ExportGroup("Deck")]
    [Export]
    public Array<CardData> StartingDeck { get; set; } = new();

    [ExportGroup("Settings")]
    [Export]
    public bool ClickDrawPileToDraw { get; set; } = true;

    public override void _Ready()
    {
        if (drawPile != null)
            RegisterPile(drawPile);
        if (handPile != null)
            RegisterPile(handPile);
        if (discardPile != null)
            RegisterPile(discardPile);

        base._Ready();

        if (StartingDeck != null && StartingDeck.Count > 0)
            ResetDeck(StartingDeck);
    }

    protected override void OnCardLeftClicked(Card card)
    {
        base.OnCardLeftClicked(card);

        if (!ClickDrawPileToDraw || drawPile == null || card == null)
            return;
        if (GetPile(card) != drawPile || card != drawPile.GetTopCard())
            return;

        DrawCard(1);
    }

    public CardPile GetPile(PileKind kind) =>
        kind switch
        {
            PileKind.Draw => drawPile,
            PileKind.Hand => handPile,
            PileKind.Discard => discardPile,
            _ => null,
        };

    public void SetCardPile(Card card, PileKind kind)
    {
        var pile = GetPile(kind);
        if (pile != null)
            MoveToPile(card, pile);
    }

    public Card GetCardInPileAt(PileKind kind, int index) => GetPile(kind)?.GetCardAt(index);

    public Array<Card> GetCardsInPile(PileKind kind) =>
        GetPile(kind)?.GetCards() ?? new Array<Card>();

    public int GetCardPileSize(PileKind kind) => GetPile(kind)?.CardsCount() ?? 0;

    public bool IsCardInHand(Card card) => handPile != null && handPile.IsHolding(card);

    public void DrawCard(int numCards = 1)
    {
        if (drawPile == null || handPile == null)
            return;

        for (int i = 0; i < numCards; i++)
        {
            var card = drawPile.GetTopCard();
            if (card == null)
                break;
            MoveToPile(card, handPile);
        }
    }

    public void DiscardCard(Card card)
    {
        if (discardPile != null)
            MoveToPile(card, discardPile);
    }

    public void ResetDeck(IEnumerable<CardData> deck)
    {
        var existing = new System.Collections.Generic.List<Card>();
        foreach (var child in GetChildren())
        {
            if (child is Card card)
                existing.Add(card);
        }
        foreach (var card in existing)
            RemoveCardFromGame(card);

        if (deck == null || drawPile == null)
            return;

        foreach (var data in deck)
        {
            if (data == null)
                continue;
            var card = CreateCard(data);
            MoveToPile(card, drawPile, updateLayout: false);
        }

        drawPile.Shuffle();
        UpdateCardsTargetPosition(instantlyMove: true);
        UpdateCardsZIndex();
        EmitSignal(SignalName.DrawPileUpdated);
        EmitSignal(SignalName.HandPileUpdated);
        EmitSignal(SignalName.DiscardPileUpdated);
    }

    protected override void OnCardAddedToPile(CardPile pile, Card card)
    {
        base.OnCardAddedToPile(pile, card);
        EmitNamedPileUpdated(pile);
    }

    protected override void OnCardRemovedFromPile(CardPile pile, Card card)
    {
        base.OnCardRemovedFromPile(pile, card);
        EmitNamedPileUpdated(pile);
    }

    void EmitNamedPileUpdated(CardPile pile)
    {
        if (pile == drawPile)
            EmitSignal(SignalName.DrawPileUpdated);
        else if (pile == handPile)
            EmitSignal(SignalName.HandPileUpdated);
        else if (pile == discardPile)
            EmitSignal(SignalName.DiscardPileUpdated);
    }
}
