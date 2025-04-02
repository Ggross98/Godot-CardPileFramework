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

    public enum DropzoneType
    {
        DrawPile,
        HandPile,
        DiscardPile,
        Dropzone
    }
    

    public override void _Ready()
    {
        base._Ready();

        LoadJsonFiles();
        ResetCardCollection();

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    public void SetCardPile(Card card, DropzoneType dropzoneType)
    {

        if(card == null) return;
        if(dropzoneType == DropzoneType.Dropzone) return;

        var pile = GetPile(dropzoneType);
        SetCardDropzone(card, pile);

        if (dropzoneType == DropzoneType.DiscardPile)
        {
            EmitSignal(nameof(DiscardPileUpdated));
        }
        else if (dropzoneType == DropzoneType.HandPile)
        {
            EmitSignal(nameof(HandPileUpdated));
        }
        else if (dropzoneType == DropzoneType.DrawPile)
        {
            EmitSignal(nameof(DrawPileUpdated));
        }
    }

    
    public CardDropzone GetPile(DropzoneType dropzoneType){
        if(dropzoneType == DropzoneType.DrawPile){
            return drawPile;
        }
        else if(dropzoneType == DropzoneType.DiscardPile){
            return discardPile;
        }
        else if(dropzoneType == DropzoneType.HandPile){
            return handPile;
        }
        else return null;
    }

    public Card GetCardInPileAt(DropzoneType dropzoneType, int index)
    {
        var pile = GetPile(dropzoneType);
        if(pile != null){
            return pile.GetCardAt(index);
        }
        else{
            return null;
        }
    }
    public Array<Card> GetCardsInPile(DropzoneType dropzoneType)
    {
        var pile = GetPile(dropzoneType);
        if(pile != null){
            return pile.GetCards();
        }
        else{
            return new Array<Card>();
        }
    }

    public int GetCardPileSize(DropzoneType dropzoneType){

        var pile = GetPile(dropzoneType);
        if(pile != null){
            return pile.GetCards().Count;
        }
        else return 0;
    
    }

    protected override void MaybeRemoveCardFromAnyDropzones(Card card)
    {
        var allDropzones = new Array<CardDropzone>();
        GetDropzones(GetTree().Root, "CardDropzone", allDropzones);
        foreach (var dropzone in allDropzones)
        {
            bool removed = false;
            if (dropzone.IsHolding(card))
            {
                // GD.Print(card.Name);
                dropzone.RemoveCard(card);
                EmitSignal(nameof(CardRemovedFromDropzone), dropzone, card);

                if(dropzone == handPile){
                    EmitSignal(SignalName.HandPileUpdated);
                }
                else if(dropzone == drawPile){
                    EmitSignal(SignalName.DrawPileUpdated);
                }
                else if(dropzone == discardPile){
                    EmitSignal(SignalName.DiscardPileUpdated);
                }

                removed = true;
            }

            if(removed) break;
        }
    }

    // protected Card CreateCardInPile(CardData cardData, DropzoneType pile)
    // {
    //     var cardUi = CreateCard(cardData);
    //     SetCardPile(cardUi, pile);

    //     return cardUi;
    // }

    protected Card CreateCardInPile(string niceName, DropzoneType dropzoneType)
    {
        if(dropzoneType == DropzoneType.Dropzone) return null;

        var json = GetCardDataByNiceName(niceName);
        var cardUi = CreateCardFromJson(json);
        SetCardPile(cardUi, dropzoneType);

        return cardUi;
    }

    protected void LoadJsonFiles()
    {   
        cardDatabase = JsonUtils.LoadJsonAs<Array<Dictionary>>(cardDatabasePath);
        cardCollection = JsonUtils.LoadJsonAs<Array<string>>(cardCollectionPath);
    }


    protected void ResetCardCollection()
    {
        foreach (Node child in GetChildren())
        {
            if(child.GetType() == typeof(Card)){
                MaybeRemoveCardFromAnyDropzones(child as Card);
                RemoveCardFromGame(child as Card);
            }
        }
        foreach (var niceName in cardCollection)
        {
            var cardData = GetCardDataByNiceName(niceName);
            var cardUi = CreateCardFromJson(cardData);
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
        while (handPile.CardsCount() > handPile.MaxHandSize)
            SetCardPile(handPile.GetTopCard(), DropzoneType.DiscardPile);
        handPile.UpdateCardsTargetPositions();
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


    public void DrawCard(int numCards = 1)
    {
        for (int i = 0; i < numCards; i++)
        {
            if (handPile.CardsCount() >= handPile.MaxHandSize && cantDrawAtHandLimit)
                continue;
            if(drawPile.CardsCount() > 0){
                var card = drawPile.GetTopCard();
                SetCardPile(card, DropzoneType.HandPile);
            }
            else if(shuffleDiscardOnEmptyDraw && discardPile.CardsCount() > 0){
                var discardCards = discardPile.GetCards().Duplicate();
                foreach(var discardCard in discardCards){
                    SetCardPile(discardCard, DropzoneType.DrawPile);
                }
                drawPile.GetCards().Shuffle();
                var card = drawPile.GetTopCard();
                SetCardPile(card, DropzoneType.HandPile);
            }
        }

        UpdateCardsTargetPosition();
    }

    public void DiscardCard(Card card){
        SetCardDropzone(card, discardPile);
    }

    public bool IsHandFull()
    {
        return handPile.IsFull();
    }

    public bool IsPileEnabled(DropzoneType dropzoneType){
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
