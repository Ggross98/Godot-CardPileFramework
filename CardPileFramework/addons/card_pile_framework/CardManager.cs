namespace Ggross.CardPileFramework;

using Godot;
using Godot.Collections;

/// <summary>
/// Base class of managers which control card objects and piles.
/// </summary>
public partial class CardManager : Control
{
    #region Signals
    [Signal]
    public delegate void CardRemovedFromDropzoneEventHandler(CardDropzone dropzone, Card card);

    [Signal]
    public delegate void CardAddedToDropzoneEventHandler(CardDropzone dropzone, Card card);

    [Signal]
    public delegate void CardHoveredEventHandler(Card card);

    [Signal]
    public delegate void CardUnhoveredEventHandler(Card card);

    [Signal]
    public delegate void CardLeftClickedEventHandler(Card card);

    [Signal]
    public delegate void CardRightClickedEventHandler(Card card);

    [Signal]
    public delegate void CardDroppedEventHandler(Card card);

    [Signal]
    public delegate void CardRemovedFromGameEventHandler(Card card);

    protected virtual void OnCardRemovedFromDropzone(CardDropzone dropzone, Card card)
    {
        EmitSignal(SignalName.CardRemovedFromDropzone, dropzone, card);
    }

    protected virtual void OnCardAddedFromDropzone(CardDropzone dropzone, Card card)
    {
        EmitSignal(SignalName.CardAddedToDropzone, dropzone, card);
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

    protected virtual void OnCardRightClicked(Card card)
    {
        EmitSignal(SignalName.CardRightClicked, card);
    }

    protected virtual void OnCardDropped(Card card)
    {
        EmitSignal(SignalName.CardDropped, card);
    }

    protected virtual void OnCardRemovedFromGame(Card card)
    {
        EmitSignal(SignalName.CardRemovedFromGame, card);
    }

    #endregion

    /// <summary>
    /// Card UI prefab
    /// </summary>
    [Export]
    protected PackedScene cardUIPrefab;

    /// <summary>
    /// Create a card UI object, then add it as child.
    /// </summary>
    /// <param name="cardData"></param>
    /// <returns></returns>
    protected Card CreateCard(CardData cardData)
    {
        var cardUi = cardUIPrefab.Instantiate<Card>();
        cardUi.Manager = this;

        // UI initialization
        cardUi.CardData = cardData;

        // Connect signals
        cardUi.CardHovered += OnCardHovered;
        cardUi.CardUnhovered += OnCardUnhovered;
        cardUi.CardLeftClicked += OnCardLeftClicked;
        cardUi.CardDropped += OnCardDropped;

        AddChild(cardUi);
        return cardUi;
    }

    protected void CreateCardInDropzone(CardData cardData, CardDropzone dropzone)
    {
        var cardUi = CreateCard(cardData);

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

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    /// <summary>
    /// Move a card UI object to specific dropzone.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="dropzone"></param>
    public virtual void SetCardDropzone(Card card, CardDropzone dropzone)
    {
        if (card == null || dropzone == null)
            return;

        MaybeRemoveCardFromAnyDropzones(card);

        dropzone.AddCard(card);
        EmitSignal(nameof(CardAddedToDropzone), dropzone, card);

        UpdateCardsTargetPosition();
        UpdateCardsZIndex();
    }

    /// <summary>
    /// Remove a card UI object from all dropzone objects.
    /// </summary>
    /// <param name="card"></param>
    protected virtual void MaybeRemoveCardFromAnyDropzones(Card card)
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

                removed = true;
            }
            if (removed)
                break;
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
    public virtual void GetDropzones(Node node, string className, Array<CardDropzone> result)
    {
        if (node is CardDropzone dropzone)
        {
            // if(dropzone.pilesType == CardDropzone.DropzoneType.Dropzone)
            //     result.Add(dropzone);
            result.Add(dropzone);
        }

        foreach (Node child in node.GetChildren())
            GetDropzones(child, className, result);
    }

    /// <summary>
    /// Update the positions.
    /// </summary>
    public virtual void UpdateCardsTargetPosition() { }

    public virtual void UpdateCardsZIndex() { }

    public bool IsAnyCardClicked()
    {
        var allDropzones = new Array<CardDropzone>();
        GetDropzones(GetTree().Root, "CardDropzone", allDropzones);

        foreach (var dropzone in allDropzones)
        {
            if (dropzone.IsAnyCardClicked())
            {
                return true;
            }
        }
        return false;
    }
}
