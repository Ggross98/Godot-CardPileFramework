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
    [Export] protected CardPile drawPile, discardPile;
    [Export] protected CardHand handPile;


    [ExportGroup("Cards")]
    [Export]
    public float cardReturnSpeed = 0.15f;

    [ExportGroup("Settings")]
    [Export] public bool clickDrawPileToDraw = true;
    [Export] public bool cantDrawAtHandLimit = true;
    [Export] public bool shuffleDiscardOnEmptyDraw = true;
    // [Export]
    // public CardDropzone.PilesCardLayouts drawPileLayout = CardDropzone.PilesCardLayouts.Up;
    
    
    [Export]
    public int cardUIHoverDistance = 30;
    [Export]
    public bool dragWhenClicked = true;
    

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
    // protected Array<Card> _handPile = new Array<Card>();
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

    public void SetCardPile(Card card, CardDropzone.DropzoneType pile)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);

        if (pile == CardDropzone.DropzoneType.DiscardPile)
        {
            // _discardPile.Add(card);
            discardPile.AddCard(card);
            EmitSignal(nameof(DiscardPileUpdated));
        }
        else if (pile == CardDropzone.DropzoneType.HandPile)
        {
            // _handPile.Add(card);
            handPile.AddCard(card);
            EmitSignal(nameof(HandPileUpdated));
        }
        else if (pile == CardDropzone.DropzoneType.DrawPile)
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

    public Array<Card> GetCardsInPile(CardDropzone.DropzoneType pile)
    {
        if (pile == CardDropzone.DropzoneType.DiscardPile)
            // return [.. _discardPile];
            return discardPile.GetHoldingCards();
        else if (pile == CardDropzone.DropzoneType.HandPile)
            // return [.. _handPile];
            return handPile.GetHoldingCards();
        else if (pile == CardDropzone.DropzoneType.DrawPile)
            // return [.. _drawPile];
            return drawPile.GetHoldingCards();

        else return new Array<Card>();
    }

    public CardDropzone GetPile(CardDropzone.DropzoneType pile){
        if(pile == CardDropzone.DropzoneType.DrawPile){
            return drawPile;
        }
        else if(pile == CardDropzone.DropzoneType.DiscardPile){
            return discardPile;
        }
        else return null;
    }

    public Card GetCardInPileAt(CardDropzone.DropzoneType pile, int index)
    {
        if(pile == CardDropzone.DropzoneType.HandPile && handPile.GetTotalHoldingCards() > index){
            return handPile.GetCardAt(index);
        }
        else if(pile == CardDropzone.DropzoneType.DrawPile){
            return drawPile.GetCardAt(index);
        }
        else if(pile == CardDropzone.DropzoneType.DiscardPile){
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

    public int GetCardPileSize(CardDropzone.DropzoneType piles){

        
        var pile = GetPile(piles);
        if(pile != null){
            return pile.GetHoldingCards().Count;
        }
        else return 0;
        

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

        if (handPile.IsHolding(card))
        {
            handPile.RemoveCard(card);
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

    protected void CreateCardInPile(string niceName, CardDropzone.DropzoneType pileToAddTo)
    {
        var cardUi = CreateCardFromJson(GetCardDataByNiceName(niceName));

        if (pileToAddTo == CardDropzone.DropzoneType.HandPile)
            cardUi.Position = handPile.Position;
        else if (pileToAddTo == CardDropzone.DropzoneType.DiscardPile)
            cardUi.Position = discardPile.Position;
        else if (pileToAddTo == CardDropzone.DropzoneType.DrawPile)
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
        drawPile.GetHoldingCards().Shuffle();

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
        handPile.UpdateCardsTargetPositions();
        while (handPile.GetTotalHoldingCards() > handPile.maxHandSize)
            SetCardPile(handPile.GetTopCard(), CardDropzone.DropzoneType.DiscardPile);
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
        handPile.UpdateCardsZIndex();
    }

    public bool IsCardInHand(Card cardUi)
    {
        return handPile.IsHolding(cardUi);
    }

    public bool IsAnyCardClicked()
    {
        foreach (var pile in new Array<CardDropzone>(){ handPile, drawPile, discardPile})
        {
            if (pile.IsAnyCardClicked()){
                return true;
            }
        }
        var allDropzones = new Array<CardDropzone>();
        GetDropzones(GetTree().Root, "CardDropzone", allDropzones);
        foreach (var dropzone in allDropzones)
        {
            if(dropzone.IsAnyCardClicked()){
                return true;
            }
        }
        return false;
    }

    public new void Draw(int numCards = 1)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (handPile.GetTotalHoldingCards() >= handPile.maxHandSize && cantDrawAtHandLimit)
                continue;
            if(drawPile.GetTotalHoldingCards() > 0){
                var card = drawPile.GetTopCard();
                SetCardPile(card, CardDropzone.DropzoneType.HandPile);
            }
            else if(shuffleDiscardOnEmptyDraw && discardPile.GetTotalHoldingCards() > 0){
                var discardCards = discardPile.GetHoldingCards().Duplicate();
                foreach(var discardCard in discardCards){
                    SetCardPile(discardCard, CardDropzone.DropzoneType.DrawPile);
                }
                drawPile.GetHoldingCards().Shuffle();
                var card = drawPile.GetTopCard();
                SetCardPile(card, CardDropzone.DropzoneType.HandPile);
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

    public void Discard(Card card){
        SetCardDropzone(card, discardPile);
        ResetTargetPositions();
    }

    public bool IsHandFull()
    {
        return handPile.IsFull();
    }

    public bool IsPileEnabled(CardDropzone.DropzoneType dropzoneType){
        var pile = GetPile(dropzoneType);
        if(pile != null){
            return pile.enabled;
        }
        return false;
    }

    public void SortHand(Func<Card, Card> sortFunc)
    {
        handPile.GetHoldingCards().OrderBy(sortFunc);
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

    protected void CreateCardInPile(CardData cardData, CardDropzone.DropzoneType pile)
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
