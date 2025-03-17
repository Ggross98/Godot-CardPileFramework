namespace Ggross.CardPileFramework;

using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class SimpleCardPileManager : Control
{
    [Signal]
    public delegate void DrawPileUpdatedEventHandler();
    [Signal]
    public delegate void HandPileUpdatedEventHandler();
    [Signal]
    public delegate void DiscardPileUpdatedEventHandler();
    [Signal]
    public delegate void CardRemovedFromDropzoneEventHandler(CardDropzone dropzone, Card card);
    [Signal]
    public delegate void CardAddedToDropzoneEventHandler(CardDropzone dropzone, Card card);
    [Signal]
    public delegate void CardHoveredEventHandler(Card card);
    [Signal]
    public delegate void CardUnhoveredEventHandler(Card card);
    [Signal]
    public delegate void CardClickedEventHandler(Card card);
    [Signal]
    public delegate void CardDroppedEventHandler(Card card);
    [Signal]
    public delegate void CardRemovedFromGameEventHandler(Card card);

    public enum Piles
    {
        DrawPile,
        HandPile,
        DiscardPile
    }

    public enum PilesCardLayouts
    {
        Up,
        Left,
        Right,
        Down
    }

    [Export(PropertyHint.File, "*.json")]
    public string JsonCardDatabasePath;
    [Export(PropertyHint.File, "*.json")]
    public string JsonCardCollectionPath;
    [Export]
    public PackedScene ExtendedCardUI;

    [ExportGroup("Pile Positions")]
    [Export]
    public Vector2 DrawPilePosition = new Vector2(20, 460);
    [Export]
    public Vector2 HandPilePosition = new Vector2(630, 460);
    [Export]
    public Vector2 DiscardPilePosition = new Vector2(1250, 460);

    [ExportGroup("Pile Displays")]
    [Export]
    public int StackDisplayGap = 8;
    [Export]
    public int MaxStackDisplay = 6;

    [ExportGroup("Cards")]
    [Export]
    public float CardReturnSpeed = 0.15f;

    [ExportGroup("Draw Pile")]
    [Export]
    public bool ClickDrawPileToDraw = true;
    [Export]
    public bool CantDrawAtHandLimit = true;
    [Export]
    public bool ShuffleDiscardOnEmptyDraw = true;
    [Export]
    public PilesCardLayouts DrawPileLayout = PilesCardLayouts.Up;

    [ExportGroup("Hand Pile")]
    [Export]
    public bool HandEnabled = true;
    [Export]
    public bool HandFaceUp = true;
    [Export]
    public int MaxHandSize = 10;
    [Export]
    public int MaxHandSpread = 700;
    [Export]
    public int CardUiHoverDistance = 30;
    [Export]
    public bool DragWhenClicked = true;
    [Export]
    public Curve HandRotationCurve;
    [Export]
    public Curve HandVerticalCurve;

    [ExportGroup("Discard Pile")]
    [Export]
    public bool DiscardFaceUp = true;
    [Export]
    public PilesCardLayouts DiscardPileLayout = PilesCardLayouts.Up;

    /// <summary>
    /// Save the base data (name, description, damage etc.) of all cards
    /// </summary>
    private Array<Dictionary> cardDatabase = new Array<Dictionary>();

    /// <summary>
    /// Save the names of present cards
    /// </summary>
    private Array<string> cardCollection = new Array<string>();

    private Array<Card> _drawPile = new Array<Card>();
    private Array<Card> _handPile = new Array<Card>();
    private Array<Card> _discardPile = new Array<Card>();

    private Curve spreadCurve = new Curve();

    public override void _Ready()
    {
        Size = Vector2.Zero;
        spreadCurve.AddPoint(new Vector2(0, -1), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        spreadCurve.AddPoint(new Vector2(1, 1), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        LoadJsonFiles();
        ResetCardCollection();
        ResetTargetPositions();
    }

    public void SetCardPile(Card card, Piles pile)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);

        if (pile == Piles.DiscardPile)
        {
            _discardPile.Add(card);
            EmitSignal(nameof(DiscardPileUpdated));
        }
        else if (pile == Piles.HandPile)
        {
            _handPile.Add(card);
            EmitSignal(nameof(HandPileUpdated));
        }
        else if (pile == Piles.DrawPile)
        {
            _drawPile.Add(card);
            EmitSignal(nameof(DrawPileUpdated));
        }

        ResetTargetPositions();
    }

    public void SetCardDropzone(Card card, CardDropzone dropzone)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);
        dropzone.AddCard(card);
        EmitSignal(nameof(CardAddedToDropzone), dropzone, card);
        ResetTargetPositions();
    }

    private void RemoveCardFromGame(Card card)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);
        EmitSignal(nameof(CardRemovedFromGame), card);
        card.QueueFree();
        ResetTargetPositions();
    }

    public Array<Card> GetCardsInPile(Piles pile)
    {
        if (pile == Piles.DiscardPile)
            return [.. _discardPile];
        if (pile == Piles.HandPile)
            return [.. _handPile];
        if (pile == Piles.DrawPile)
            return [.. _drawPile];

        return new Array<Card>();
    }

    public Card GetCardInPileAt(Piles pile, int index)
    {
        if (pile == Piles.DiscardPile && _discardPile.Count > index)
            return _discardPile[index];
        if (pile == Piles.DrawPile && _drawPile.Count > index)
            return _drawPile[index];
        if (pile == Piles.HandPile && _handPile.Count > index)
            return _handPile[index];
        return null;
    }

    public int GetCardPileSize(Piles piles){
        if (piles == Piles.DiscardPile)
            return _discardPile.Count;
        if (piles == Piles.HandPile)
            return _handPile.Count;
        if (piles == Piles.DrawPile)
            return _drawPile.Count;
        return 0;
    }

    private void MaybeRemoveCardFromAnyPiles(Card card)
    {
        if (_handPile.Contains(card))
        {
            _handPile.Remove(card);
            EmitSignal(nameof(HandPileUpdated));
        }
        else if (_drawPile.Contains(card))
        {
            _drawPile.Remove(card);
            EmitSignal(nameof(DrawPileUpdated));
        }
        else if (_discardPile.Contains(card))
        {
            _discardPile.Remove(card);
            EmitSignal(nameof(DiscardPileUpdated));
        }
    }

    private void CreateCardInDropzone(string niceName, CardDropzone dropzone)
    {
        var cardUi = CreateCardUI(GetCardDataByNiceName(niceName));
        cardUi.Position = dropzone.Position;
        SetCardDropzone(cardUi, dropzone);
    }

    private void CreateCardInPile(string niceName, Piles pileToAddTo)
    {
        var cardUi = CreateCardUI(GetCardDataByNiceName(niceName));
        if (pileToAddTo == Piles.HandPile)
            cardUi.Position = HandPilePosition;
        else if (pileToAddTo == Piles.DiscardPile)
            cardUi.Position = DiscardPilePosition;
        else if (pileToAddTo == Piles.DrawPile)
            cardUi.Position = DrawPilePosition;
        SetCardPile(cardUi, pileToAddTo);
    }

    private void MaybeRemoveCardFromAnyDropzones(Card card)
    {
        var allDropzones = new Array<CardDropzone>();
        GetDropzones(GetTree().Root, "CardDropzone", allDropzones);
        foreach (var dropzone in allDropzones)
        {
            if (dropzone.IsHolding(card))
            {
                dropzone.RemoveCard(card);
                EmitSignal(nameof(CardRemovedFromDropzone), dropzone, card);
            }
        }
    }

    public CardDropzone GetCardDropzone(Card card)
    {
        var allDropzones = new Array<CardDropzone>();
        GetDropzones(GetTree().Root, "CardDropzone", allDropzones);
        foreach (var dropzone in allDropzones)
        {
            if (dropzone.IsHolding(card))
                return dropzone;
        }
        return null;
    }

    private void GetDropzones(Node node, string className, Array<CardDropzone> result)
    {
        if (node is CardDropzone dropzone)
            result.Add(dropzone);
        foreach (Node child in node.GetChildren())
            GetDropzones(child, className, result);
    }

    private void LoadJsonFiles()
    {   
        cardDatabase = JsonUtils.LoadJsonAs<Array<Dictionary>>(JsonCardDatabasePath);
        cardCollection = JsonUtils.LoadJsonAs<Array<string>>(JsonCardCollectionPath);
    }


    public void Reset()
    {
        ResetCardCollection();
    }

    private void ResetCardCollection()
    {
        foreach (Node child in GetChildren())
        {
            MaybeRemoveCardFromAnyPiles(child as Card);
            MaybeRemoveCardFromAnyDropzones(child as Card);
            RemoveCardFromGame(child as Card);
        }
        foreach (var niceName in cardCollection)
        {
            var cardData = GetCardDataByNiceName(niceName);
            var cardUi = CreateCardUI(cardData);
            _drawPile.Add(cardUi);
            _drawPile.Shuffle();
        }
        SetDrawPileTargetPositions(true);
        EmitSignal(nameof(DrawPileUpdated));
        EmitSignal(nameof(HandPileUpdated));
        EmitSignal(nameof(DiscardPileUpdated));
    }

    private void ResetTargetPositions()
    {
        SetDrawPileTargetPositions();
        SetHandPileTargetPositions();
        SetDiscardPileTargetPositions();
    }

    private void SetDrawPileTargetPositions(bool instantlyMove = false)
    {
        for (int i = 0; i < _drawPile.Count; i++)
        {
            var cardUi = _drawPile[i];
            var targetPos = DrawPilePosition;
            switch (DrawPileLayout)
            {
                case PilesCardLayouts.Up:
                    targetPos.Y -= i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
                case PilesCardLayouts.Down:
                    targetPos.Y += i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
                case PilesCardLayouts.Right:
                    targetPos.X += i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
                case PilesCardLayouts.Left:
                    targetPos.X -= i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
            }
            cardUi.ZIndex = i;
            cardUi.Rotation = 0;
            cardUi.targetPosition = targetPos;
            cardUi.SetDirection(Vector2.Down);
            if (instantlyMove)
                cardUi.Position = targetPos;
        }
    }

    private void SetHandPileTargetPositions()
    {
        for (int i = 0; i < _handPile.Count; i++)
        {
            var cardUi = _handPile[i];
            cardUi.MoveToFront();
            var handRatio = _handPile.Count > 1 ? (float)i / (_handPile.Count - 1) : 0.5f;
            var targetPos = HandPilePosition;
            var cardSpacing = MaxHandSpread / (_handPile.Count + 1);
            targetPos.X += (i + 1) * cardSpacing - MaxHandSpread / 2.0f;
            if (HandVerticalCurve != null)
                targetPos.Y -= HandVerticalCurve.SampleBaked(handRatio);
            if (HandRotationCurve != null)
                cardUi.Rotation = Mathf.DegToRad(HandRotationCurve.SampleBaked(handRatio));
            cardUi.SetDirection(HandFaceUp ? Vector2.Up : Vector2.Down);
            cardUi.targetPosition = targetPos;
        }
        while (_handPile.Count > MaxHandSize)
            SetCardPile(_handPile[_handPile.Count - 1], Piles.DiscardPile);
        ResetHandPileZIndex();
    }

    private void SetDiscardPileTargetPositions()
    {
        for (int i = 0; i < _discardPile.Count; i++)
        {
            var cardUi = _discardPile[i];
            var targetPos = DiscardPilePosition;
            switch (DiscardPileLayout)
            {
                case PilesCardLayouts.Up:
                    targetPos.Y -= i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
                case PilesCardLayouts.Down:
                    targetPos.Y += i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
                case PilesCardLayouts.Right:
                    targetPos.X += i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
                case PilesCardLayouts.Left:
                    targetPos.X -= i <= MaxStackDisplay ? i * StackDisplayGap : StackDisplayGap * MaxStackDisplay;
                    break;
            }
            cardUi.SetDirection(DiscardFaceUp ? Vector2.Up : Vector2.Down);
            cardUi.ZIndex = i;
            cardUi.Rotation = 0;
            cardUi.targetPosition = targetPos;
        }
    }

    public void ResetCardUiZIndex()
    {
        for (int i = 0; i < _drawPile.Count; i++)
            _drawPile[i].ZIndex = i;
        for (int i = 0; i < _discardPile.Count; i++)
            _discardPile[i].ZIndex = i;
        ResetHandPileZIndex();
    }

    private void ResetHandPileZIndex()
    {
        for (int i = 0; i < _handPile.Count; i++)
        {
            var cardUi = _handPile[i];
            cardUi.ZIndex = 1000 + i;
            cardUi.MoveToFront();
            if (cardUi.mouseIsHovering)
                cardUi.ZIndex = 2000 + i;
            if (cardUi.isClicked)
                cardUi.ZIndex = 3000 + i;
        }
    }

    public bool IsCardInHand(Card cardUi)
    {
        return _handPile.Contains(cardUi);
    }

    public bool IsAnyCardUiClicked()
    {
        foreach (var cardUi in _handPile)
        {
            if (cardUi.isClicked)
                return true;
        }
        var allDropzones = new Array<CardDropzone>();
        GetDropzones(GetTree().Root, "CardDropzone", allDropzones);
        foreach (var dropzone in allDropzones)
        {
            foreach (var card in dropzone.GetHeldCards())
            {
                if (card.isClicked)
                    return true;
            }
        }
        return false;
    }

    public new void Draw(int numCards = 1)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (_handPile.Count >= MaxHandSize && CantDrawAtHandLimit)
                continue;
            if (_drawPile.Count > 0)
            {
                SetCardPile(_drawPile[_drawPile.Count - 1], Piles.HandPile);
            }
            else if (ShuffleDiscardOnEmptyDraw && _discardPile.Count > 0)
            {
                var dupeDiscard = new Array<Card>(_discardPile);
                foreach (var c in dupeDiscard)
                    SetCardPile(c, Piles.DrawPile);
                _drawPile.Shuffle();
                SetCardPile(_drawPile[_drawPile.Count - 1], Piles.HandPile);
            }
        }
        ResetTargetPositions();
    }

    public bool HandIsAtMaxCapacity()
    {
        return _handPile.Count >= MaxHandSize;
    }

    public void SortHand(Func<Card, Card> sortFunc)
    {
        _handPile.OrderBy(sortFunc);
        ResetTargetPositions();
    }

    private Card CreateCardUI(Dictionary jsonData)
    {
        var cardUi = ExtendedCardUI.Instantiate<Card>();
        
        // Card data initialilzation
        var script = (CSharpScript)ResourceLoader.Load(jsonData["resource_script_path"].As<string>());
        var cardData = script.New().As<CardData>();
        cardData.LoadProperties(jsonData);
        
        // UI initialization
        cardUi.cardData = cardData;
        cardUi.UpdateDisplay();
        cardUi.SetControlParameters(CardReturnSpeed, CardUiHoverDistance, DragWhenClicked);

        // Connect signals
        cardUi.CardHovered += OnCardHovered;
        cardUi.CardUnhovered += OnCardUnhovered;
        cardUi.CardClicked += OnCardClicked;
        cardUi.CardDropped += OnCardDropped;

        AddChild(cardUi);
        return cardUi;
    }

    private Dictionary GetCardDataByNiceName(string niceName)
    {
        foreach (var jsonData in cardDatabase)
        {
            if (jsonData["nice_name"].As<string>() == niceName)
                return jsonData;
        }
        return null;
    }

    private void OnCardHovered(Card cardUi) => EmitSignal(nameof(CardHovered), cardUi);
    private void OnCardUnhovered(Card cardUi) => EmitSignal(nameof(CardUnhovered), cardUi);
    private void OnCardClicked(Card cardUi) => EmitSignal(nameof(CardClicked), cardUi);
    private void OnCardDropped(Card cardUi) => EmitSignal(nameof(CardDropped), cardUi);
}
