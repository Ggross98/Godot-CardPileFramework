namespace Ggross.CardPileFramework;

using Godot;
using System;
using Godot.Collections;
using System.Linq;

/// <summary>
/// Implementation of a simple card pile manager considering draw, discard and hand piles
/// </summary>
public partial class SimpleCardPileManager : CardPileManager
{   
    #region Signals
    [Signal] public delegate void DrawPileUpdatedEventHandler();
    [Signal] public delegate void HandPileUpdatedEventHandler();
    [Signal] public delegate void DiscardPileUpdatedEventHandler();
    #endregion



    [ExportGroup("Create Cards")]
    [Export(PropertyHint.File, "*.json")]
    public string cardDatabasePath, cardCollectionPath;

    [ExportGroup("Piles")]
    [Export] protected CardDropzone drawPile, discardPile;
    

    [ExportGroup("Pile Positions")]
    // [Export] public Vector2 drawPilePosition = new Vector2(20, 460);
    [Export] public Vector2 handPilePosition = new Vector2(630, 460);
    // [Export] public Vector2 discardPilePosition = new Vector2(1250, 460);

    [ExportGroup("Pile Displays")]
    [Export]
    public int stackDisplayGap = 8;
    [Export]
    public int maxStackDisplay = 6;

    [ExportGroup("Cards")]
    [Export]
    public float cardReturnSpeed = 0.15f;

    [ExportGroup("Pile Settings")]
    [Export] public bool clickDrawPileToDraw = true;
    [Export] public bool cantDrawAtHandLimit = true;
    [Export] public bool shuffleDiscardOnEmptyDraw = true;
    // [Export]
    // public CardDropzone.PilesCardLayouts drawPileLayout = CardDropzone.PilesCardLayouts.Up;
    [Export]
    public bool handEnabled = true;
    [Export]
    public bool handFaceUp = true;
    [Export]
    public int maxHandSize = 10;
    [Export]
    public int maxHandSpread = 700;
    [Export]
    public int cardUIHoverDistance = 30;
    [Export]
    public bool dragWhenClicked = true;
    [Export]
    public Curve handRotationCurve;
    [Export]
    public Curve handVerticalCurve;

    // [ExportGroup("Discard Pile")]
    // [Export]
    // public bool discardFaceUp = true;
    // [Export]
    // public CardDropzone.PilesCardLayouts discardPileLayout = CardDropzone.PilesCardLayouts.Up;

    /// <summary>
    /// Save the base data of all cards
    /// </summary>
    protected Array<Dictionary> cardDatabase = new Array<Dictionary>();

    /// <summary>
    /// Save the names of present cards (i.e. the deck) for indexing
    /// </summary>
    protected Array<string> cardCollection = new Array<string>();

    // protected Array<Card> _drawPile = new Array<Card>();
    protected Array<Card> _handPile = new Array<Card>();
    // protected Array<Card> _discardPile = new Array<Card>();

    /// <summary>
    /// Show curve when player drags a card
    /// </summary>
    protected Curve spreadCurve = new Curve();

    public override void _Ready()
    {
        base._Ready();

        Size = Vector2.Zero;
        spreadCurve.AddPoint(new Vector2(0, -1), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        spreadCurve.AddPoint(new Vector2(1, 1), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        LoadJsonFiles();
        ResetCardCollection();
        ResetTargetPositions();
    }

    public void SetCardPile(Card card, CardDropzone.PilesType pile)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);

        if (pile == CardDropzone.PilesType.DiscardPile)
        {
            // _discardPile.Add(card);
            discardPile.AddCard(card);
            EmitSignal(nameof(DiscardPileUpdated));
        }
        else if (pile == CardDropzone.PilesType.HandPile)
        {
            _handPile.Add(card);
            EmitSignal(nameof(HandPileUpdated));
        }
        else if (pile == CardDropzone.PilesType.DrawPile)
        {
            // _drawPile.Add(card);
            drawPile.AddCard(card);
            EmitSignal(nameof(DrawPileUpdated));
        }

        ResetTargetPositions();
    }

    public override void SetCardDropzone(Card card, CardDropzone dropzone)
    {
        MaybeRemoveCardFromAnyPiles(card);

        base.SetCardDropzone(card, dropzone);
    }

    public override void RemoveCardFromGame(Card card)
    {
        MaybeRemoveCardFromAnyPiles(card);

        base.RemoveCardFromGame(card);
    }

    public Array<Card> GetCardsInPile(CardDropzone.PilesType pile)
    {
        if (pile == CardDropzone.PilesType.DiscardPile)
            // return [.. _discardPile];
            return discardPile.GetHeldCards();
        else if (pile == CardDropzone.PilesType.HandPile)
            return [.. _handPile];
        else if (pile == CardDropzone.PilesType.DrawPile)
            // return [.. _drawPile];
            return drawPile.GetHeldCards();

        else return new Array<Card>();
    }

    public CardDropzone GetPile(CardDropzone.PilesType pile){
        if(pile == CardDropzone.PilesType.DrawPile){
            return drawPile;
        }
        else if(pile == CardDropzone.PilesType.DiscardPile){
            return discardPile;
        }
        else return null;
    }

    public Card GetCardInPileAt(CardDropzone.PilesType pile, int index)
    {
        if(pile == CardDropzone.PilesType.HandPile && _handPile.Count > index){
            return _handPile[index];
        }
        else if(pile == CardDropzone.PilesType.DrawPile){
            return drawPile.GetCardAt(index);
        }
        else if(pile == CardDropzone.PilesType.DiscardPile){
            return discardPile.GetCardAt(index);
        }
        else return null;

        // if (pile == PilesType.DiscardPile && _discardPile.Count > index)
        //     return _discardPile[index];
        // if (pile == PilesType.DrawPile && _drawPile.Count > index)
        //     return _drawPile[index];
        // if (pile == PilesType.HandPile && _handPile.Count > index)
        //     return _handPile[index];
        // return null;
    }

    public int GetCardPileSize(CardDropzone.PilesType piles){

        if(piles == CardDropzone.PilesType.HandPile){
            return _handPile.Count;
        }
        else{
            var pile = GetPile(piles);
            if(pile != null){
                return pile.GetHeldCards().Count;
            }
            else return 0;
        }

        // if (piles == PilesType.DiscardPile)
        //     return _discardPile.Count;
        // if (piles == PilesType.HandPile)
        //     return _handPile.Count;
        // if (piles == PilesType.DrawPile)
        //     return _drawPile.Count;
        // return 0;
    }

    protected void MaybeRemoveCardFromAnyPiles(Card card)
    {

        if (_handPile.Contains(card))
        {
            _handPile.Remove(card);
            EmitSignal(SignalName.HandPileUpdated);
        }
        else if(drawPile.IsHolding(card)){
            drawPile.RemoveCard(card);
            EmitSignal(SignalName.DrawPileUpdated);
        }
        else if(discardPile.IsHolding(card)){
            discardPile.RemoveCard(card);
            EmitSignal(SignalName.DiscardPileUpdated);
        }

        // if (_handPile.Contains(card))
        // {
        //     _handPile.Remove(card);
        //     EmitSignal(nameof(HandPileUpdated));
        // }
        // else if (_drawPile.Contains(card))
        // {
        //     _drawPile.Remove(card);
        //     EmitSignal(nameof(DrawPileUpdated));
        // }
        // else if (_discardPile.Contains(card))
        // {
        //     _discardPile.Remove(card);
        //     EmitSignal(nameof(DiscardPileUpdated));
        // }
    }

    protected void CreateCardInPile(string niceName, CardDropzone.PilesType pileToAddTo)
    {
        var cardUi = CreateCardFromJson(GetCardDataByNiceName(niceName));

        if (pileToAddTo == CardDropzone.PilesType.HandPile)
            cardUi.Position = handPilePosition;
        else if (pileToAddTo == CardDropzone.PilesType.DiscardPile)
            cardUi.Position = discardPile.Position;
        else if (pileToAddTo == CardDropzone.PilesType.DrawPile)
            cardUi.Position = drawPile.Position;

        SetCardPile(cardUi, pileToAddTo);
    }

    protected void LoadJsonFiles()
    {   
        cardDatabase = JsonUtils.LoadJsonAs<Array<Dictionary>>(cardDatabasePath);
        cardCollection = JsonUtils.LoadJsonAs<Array<string>>(cardCollectionPath);
    }


    public void Reset()
    {
        ResetCardCollection();
    }

    protected void ResetCardCollection()
    {
        foreach (Node child in GetChildren())
        {
            if(child.GetType() == typeof(Card)){
                MaybeRemoveCardFromAnyPiles(child as Card);
                MaybeRemoveCardFromAnyDropzones(child as Card);
                RemoveCardFromGame(child as Card);
            }
        }
        foreach (var niceName in cardCollection)
        {
            var cardData = GetCardDataByNiceName(niceName);
            var cardUi = CreateCardFromJson(cardData);
            
            // _drawPile.Add(cardUi);
            // _drawPile.Shuffle();
            drawPile.AddCard(cardUi);
        }
        drawPile.GetHeldCards().Shuffle();

        ResetTargetPositions();

        EmitSignal(nameof(DrawPileUpdated));
        EmitSignal(nameof(HandPileUpdated));
        EmitSignal(nameof(DiscardPileUpdated));
    }

    protected override void ResetTargetPositions()
    {
        SetDrawPileTargetPositions();
        SetHandPileTargetPositions();
        SetDiscardPileTargetPositions();
    }

    protected void SetDrawPileTargetPositions(bool instantlyMove = false)
    {
        drawPile.UpdateCardsTargetPositions(instantlyMove);
        // for (int i = 0; i < _drawPile.Count; i++)
        // {
        //     var cardUi = _drawPile[i];
        //     var targetPos = drawPilePosition;
        //     switch (drawPileLayout)
        //     {
        //         case PilesCardLayouts.Up:
        //             targetPos.Y -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //         case PilesCardLayouts.Down:
        //             targetPos.Y += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //         case PilesCardLayouts.Right:
        //             targetPos.X += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //         case PilesCardLayouts.Left:
        //             targetPos.X -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //     }
        //     cardUi.ZIndex = i;
        //     cardUi.Rotation = 0;
        //     cardUi.targetPosition = targetPos;
        //     cardUi.SetDirection(Vector2.Down);
        //     if (instantlyMove)
        //         cardUi.Position = targetPos;
        // }
    }

    protected void SetHandPileTargetPositions()
    {
        for (int i = 0; i < _handPile.Count; i++)
        {
            var cardUi = _handPile[i];
            cardUi.MoveToFront();
            var handRatio = _handPile.Count > 1 ? (float)i / (_handPile.Count - 1) : 0.5f;
            var targetPos = handPilePosition;
            var cardSpacing = maxHandSpread / (_handPile.Count + 1);
            targetPos.X += (i + 1) * cardSpacing - maxHandSpread / 2.0f;
            if (handVerticalCurve != null)
                targetPos.Y -= handVerticalCurve.SampleBaked(handRatio);
            if (handRotationCurve != null)
                cardUi.Rotation = Mathf.DegToRad(handRotationCurve.SampleBaked(handRatio));
            cardUi.SetDirection(handFaceUp ? Vector2.Up : Vector2.Down);
            cardUi.targetPosition = targetPos;
        }
        while (_handPile.Count > maxHandSize)
            SetCardPile(_handPile[_handPile.Count - 1], CardDropzone.PilesType.DiscardPile);
        ResetHandPileZIndex();
    }

    protected void SetDiscardPileTargetPositions()
    {
        discardPile.UpdateCardsTargetPositions();
        // for (int i = 0; i < _discardPile.Count; i++)
        // {
        //     var cardUi = _discardPile[i];
        //     var targetPos = discardPilePosition;
        //     switch (discardPileLayout)
        //     {
        //         case PilesCardLayouts.Up:
        //             targetPos.Y -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //         case PilesCardLayouts.Down:
        //             targetPos.Y += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //         case PilesCardLayouts.Right:
        //             targetPos.X += i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //         case PilesCardLayouts.Left:
        //             targetPos.X -= i <= maxStackDisplay ? i * stackDisplayGap : stackDisplayGap * maxStackDisplay;
        //             break;
        //     }
        //     cardUi.SetDirection(discardFaceUp ? Vector2.Up : Vector2.Down);
        //     cardUi.ZIndex = i;
        //     cardUi.Rotation = 0;
        //     cardUi.targetPosition = targetPos;
        // }
    }

    public void ResetCardsZIndex()
    {
        // for (int i = 0; i < _drawPile.Count; i++)
        //     _drawPile[i].ZIndex = i;
        // for (int i = 0; i < _discardPile.Count; i++)
        //     _discardPile[i].ZIndex = i;
        ResetHandPileZIndex();

        drawPile.UpdateCardsZIndex();
        discardPile.UpdateCardsZIndex();
    }

    protected void ResetHandPileZIndex()
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
            if (_handPile.Count >= maxHandSize && cantDrawAtHandLimit)
                continue;
            if(drawPile.GetTotalHeldCards() > 0){
                var card = drawPile.GetTopCard();
                SetCardPile(card, CardDropzone.PilesType.HandPile);
            }
            else if(shuffleDiscardOnEmptyDraw && discardPile.GetTotalHeldCards() > 0){
                var discardCards = discardPile.GetHeldCards().Duplicate();
                foreach(var discardCard in discardCards){
                    SetCardPile(discardCard, CardDropzone.PilesType.DrawPile);
                }
                drawPile.GetHeldCards().Shuffle();
                var card = drawPile.GetTopCard();
                SetCardPile(card, CardDropzone.PilesType.HandPile);
            }
            // if (_drawPile.Count > 0)
            // {
            //     SetCardPile(_drawPile[_drawPile.Count - 1], PilesType.HandPile);
            // }
            // else if (shuffleDiscardOnEmptyDraw && _discardPile.Count > 0)
            // {
            //     var dupeDiscard = new Array<Card>(_discardPile);
            //     foreach (var c in dupeDiscard)
            //         SetCardPile(c, PilesType.DrawPile);
            //     _drawPile.Shuffle();
            //     SetCardPile(_drawPile[_drawPile.Count - 1], PilesType.HandPile);
            // }
        }

        ResetTargetPositions();
    }

    public bool IsHandFull()
    {
        return _handPile.Count >= maxHandSize;
    }

    public void SortHand(Func<Card, Card> sortFunc)
    {
        _handPile.OrderBy(sortFunc);
        ResetTargetPositions();
    }

    protected Card CreateCardFromJson(Dictionary jsonData)
    {
        
        // Card data initialilzation
        var script = (CSharpScript)ResourceLoader.Load(jsonData["resource_script_path"].As<string>());
        var cardData = script.New().As<CardData>();
        cardData.LoadProperties(jsonData);

        var cardUi = CreateCard(cardData);
        
        cardUi.SetControlParameters(cardReturnSpeed, cardUIHoverDistance, dragWhenClicked);

        return cardUi;
    }

    protected void CreateCardInPile(CardData cardData, CardDropzone.PilesType pile)
    {
        var cardUi = CreateCard(cardData);
        SetCardPile(cardUi, pile);
    }

    protected Dictionary GetCardDataByNiceName(string niceName)
    {
        foreach (var jsonData in cardDatabase)
        {
            if (jsonData["nice_name"].As<string>() == niceName)
                return jsonData;
        }
        return null;
    }

    
}
