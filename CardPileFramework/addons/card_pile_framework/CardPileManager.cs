namespace Ggross.CardPileFramework;

using Godot;
using Godot.Collections;
using System;
using System.ComponentModel;


/// <summary>
/// Base class of managers which control card objects and piles.
/// </summary>
public partial class CardPileManager : Control
{
    #region Signals
    [Signal] public delegate void CardRemovedFromDropzoneEventHandler(CardDropzone dropzone, Card card);
    [Signal] public delegate void CardAddedToDropzoneEventHandler(CardDropzone dropzone, Card card);
    [Signal] public delegate void CardHoveredEventHandler(Card card);
    [Signal] public delegate void CardUnhoveredEventHandler(Card card);
    [Signal] public delegate void CardClickedEventHandler(Card card);
    [Signal] public delegate void CardDroppedEventHandler(Card card);
    [Signal] public delegate void CardRemovedFromGameEventHandler(Card card);

    protected void OnCardRemovedFromDropzone(CardDropzone dropzone, Card card) 
        => EmitSignal(SignalName.CardRemovedFromDropzone, dropzone, card);
    protected void OnCardAddedFromDropzone(CardDropzone dropzone, Card card) 
        => EmitSignal(SignalName.CardAddedToDropzone, dropzone, card);
    protected void OnCardHovered(Card card) 
        => EmitSignal(SignalName.CardHovered, card);
    protected void OnCardUnhovered(Card card) 
        => EmitSignal(SignalName.CardUnhovered, card);
    protected void OnCardClicked(Card card) 
        => EmitSignal(SignalName.CardClicked, card);
    protected void OnCardDropped(Card card) 
        => EmitSignal(SignalName.CardDropped, card);
    protected void OnCardRemovedFromGame(Card card) 
        => EmitSignal(SignalName.CardRemovedFromGame, card);
    #endregion

    /// <summary>
    /// Card UI prefab
    /// </summary>
    [Export] protected PackedScene cardUIPrefab;

    /// <summary>
    /// Create a card UI object, then add it as child.
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    protected Card CreateCard(CardData cardData)
    {
        var cardUi = cardUIPrefab.Instantiate<Card>();
        
        // UI initialization
        cardUi.cardData = cardData;
        cardUi.UpdateDisplay();

        // Connect signals
        cardUi.CardHovered += OnCardHovered;
        cardUi.CardUnhovered += OnCardUnhovered;
        cardUi.CardClicked += OnCardClicked;
        cardUi.CardDropped += OnCardDropped;

        AddChild(cardUi);
        return cardUi;
    }

    protected void CreateCardInDropzone(CardData cardData, CardDropzone dropzone)
    {
        var cardUi = CreateCard(cardData);
        cardUi.Position = dropzone.Position;
        SetCardDropzone(cardUi, dropzone);
    }

    /// <summary>
    /// Delete a card UI object.
    /// </summary>
    /// <param name="card"></param>
    public virtual void RemoveCardFromGame(Card card)
    {
       
        MaybeRemoveCardFromAnyDropzones(card);
        EmitSignal(nameof(CardRemovedFromGame), card);
        card.QueueFree();

        ResetTargetPositions();
    }

    /// <summary>
    /// Move a card UI object to specific dropzone.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="dropzone"></param>
    public virtual void SetCardDropzone(Card card, CardDropzone dropzone){
        
        MaybeRemoveCardFromAnyDropzones(card);
        dropzone.AddCard(card);
        EmitSignal(nameof(CardAddedToDropzone), dropzone, card);

        ResetTargetPositions();
    }

    /// <summary>
    /// Remove a card UI object from all dropzone objects.
    /// </summary>
    /// <param name="card"></param>
    protected void MaybeRemoveCardFromAnyDropzones(Card card)
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

    /// <summary>
    /// Get the dropzone which is holding the specific card UI object.
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Get all dropzone objects under the node.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="className"></param>
    /// <param name="result"></param>
    protected void GetDropzones(Node node, string className, Array<CardDropzone> result)
    {
        if (node is CardDropzone dropzone)
            result.Add(dropzone);
        foreach (Node child in node.GetChildren())
            GetDropzones(child, className, result);
    }

    /// <summary>
    /// Update the positions.
    /// </summary>
    protected virtual void ResetTargetPositions(){
        
    }
}
