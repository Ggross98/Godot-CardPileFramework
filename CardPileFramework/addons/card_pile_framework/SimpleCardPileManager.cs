namespace Ggross.CardPileFramework;

using Godot;
using System;
using Godot.Collections;
using System.Linq;

/// <summary>
/// Implementation of a simple card pile manager considering draw, discard and hand piles
/// </summary>
public partial class SimpleCardPileManager : CardManager
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
    [Export] protected SimpleCardPile drawPile, discardPile;
    [Export] protected SimpleCardHand handPile;


    [ExportGroup("Settings")]
    [Export] public float cardReturnSpeed = 0.15f;
    [Export] public int cardUIHoverDistance = 30;
    [Export] public bool clickDrawPileToDraw = true;
    [Export] public bool cantDrawAtHandLimit = true;
    [Export] public bool shuffleDiscardOnEmptyDraw = true;
    [Export] public bool dragWhenClicked = true;
    // [Export] protected Curve spreadCurve = new Curve();

    /// <summary>
    /// Save the base data of all cards
    /// </summary>
    protected Array<Dictionary> cardDatabase = new Array<Dictionary>();

    /// <summary>
    /// Save the names of present cards (i.e. the deck) for indexing
    /// </summary>
    protected Array<string> cardCollection = new Array<string>();

    

    public override void _Ready()
    {
        base._Ready();

        Size = Vector2.Zero;
        // spreadCurve.AddPoint(new Vector2(0, -1), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        // spreadCurve.AddPoint(new Vector2(1, 1), 0, 0, Curve.TangentMode.Linear, Curve.TangentMode.Linear);
        LoadJsonFiles();
        ResetCardCollection();
        UpdateCardsTargetPosition();
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

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    public override void SetCardDropzone(Card card, CardDropzone dropzone)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);

        base.SetCardDropzone(card, dropzone);

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    public override void RemoveCardFromGame(Card card)
    {
        MaybeRemoveCardFromAnyPiles(card);
        MaybeRemoveCardFromAnyDropzones(card);

        base.RemoveCardFromGame(card);

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    public Array<Card> GetCardsInPile(CardDropzone.DropzoneType pile)
    {
        if (pile == CardDropzone.DropzoneType.DiscardPile)
            // return [.. _discardPile];
            return discardPile.GetCards();
        else if (pile == CardDropzone.DropzoneType.HandPile)
            // return [.. _handPile];
            return handPile.GetCards();
        else if (pile == CardDropzone.DropzoneType.DrawPile)
            // return [.. _drawPile];
            return drawPile.GetCards();

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
        if(pile == CardDropzone.DropzoneType.HandPile && handPile.CardsCount() > index){
            return handPile.GetCardAt(index);
        }
        else if(pile == CardDropzone.DropzoneType.DrawPile){
            return drawPile.GetCardAt(index);
        }
        else if(pile == CardDropzone.DropzoneType.DiscardPile){
            return discardPile.GetCardAt(index);
        }
        else return null;
    }

    public int GetCardPileSize(CardDropzone.DropzoneType piles){

        var pile = GetPile(piles);
        if(pile != null){
            return pile.GetCards().Count;
        }
        else return 0;
    
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
        drawPile.GetCards().Shuffle();

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();

        EmitSignal(nameof(DrawPileUpdated));
        EmitSignal(nameof(HandPileUpdated));
        EmitSignal(nameof(DiscardPileUpdated));
    }

    public override void UpdateCardsTargetPosition()
    {
        drawPile.UpdateCardsTargetPositions();
        handPile.UpdateCardsTargetPositions();
        while (handPile.CardsCount() > handPile.MaxHandSize)
            SetCardPile(handPile.GetTopCard(), CardDropzone.DropzoneType.DiscardPile);
        discardPile.UpdateCardsTargetPositions();
    }

    public override void UpdateCardsZIndex()
    {
        handPile.UpdateCardsZIndex();
        drawPile.UpdateCardsZIndex();
        discardPile.UpdateCardsZIndex();
    }

    public bool IsCardInHand(Card cardUi)
    {
        return handPile.IsHolding(cardUi);
    }

    

    public new void Draw(int numCards = 1)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (handPile.CardsCount() >= handPile.MaxHandSize && cantDrawAtHandLimit)
                continue;
            if(drawPile.CardsCount() > 0){
                var card = drawPile.GetTopCard();
                SetCardPile(card, CardDropzone.DropzoneType.HandPile);
            }
            else if(shuffleDiscardOnEmptyDraw && discardPile.CardsCount() > 0){
                var discardCards = discardPile.GetCards().Duplicate();
                foreach(var discardCard in discardCards){
                    SetCardPile(discardCard, CardDropzone.DropzoneType.DrawPile);
                }
                drawPile.GetCards().Shuffle();
                var card = drawPile.GetTopCard();
                SetCardPile(card, CardDropzone.DropzoneType.HandPile);
            }
        }

        UpdateCardsTargetPosition();
    }

    public void Discard(Card card){
        SetCardDropzone(card, discardPile);
    }

    public bool IsHandFull()
    {
        return handPile.IsFull();
    }

    public bool IsPileEnabled(CardDropzone.DropzoneType dropzoneType){
        var pile = GetPile(dropzoneType);
        if(pile != null){
            return pile.IsInteractive();
        }
        return false;
    }

    public void SortHand(Func<Card, Card> sortFunc)
    {
        handPile.GetCards().OrderBy(sortFunc);
        UpdateCardsTargetPosition();
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
