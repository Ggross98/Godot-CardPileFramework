using Ggross.CardPileFramework;
using Godot;

public partial class CardBattle : Node2D
{
    [Export]
    private SimpleCardPileManager cardPileManager;

    [Export]
    private Label energyLabel,
        shieldLabel;

    [Export]
    private Button endTurnButton;

    [Export]
    private PanelContainer descriptionPanel;

    [Export]
    private RichTextLabel descriptionLabel;

    [Export]
    private Line2D targetingLine;

    [Export(PropertyHint.File, "*.json")]
    private string cardDatabasePath =
        "res://examples/card_battle/card_data/example_card_database.json";

    [Export(PropertyHint.File, "*.json")]
    private string cardCollectionPath =
        "res://examples/card_battle/card_data/example_card_collection.json";

    public const int TURN_ENERGY = 4,
        MAX_HP = 50,
        TURN_DRAW = 5;
    private int energy,
        shield,
        hp;
    public int Energy
    {
        get => energy;
        set
        {
            energy = value;
            UpdateDisplay();
        }
    }
    public int Shield
    {
        get => shield;
        set
        {
            shield = value;
            UpdateDisplay();
        }
    }
    public int HP
    {
        get => hp;
        set
        {
            hp = value;
            if (hp < 0)
                hp = 0;
            if (hp > MAX_HP)
                hp = MAX_HP;
            UpdateDisplay();
        }
    }

    private MyCard hoveringCard;
    private bool _suspendDiscardRecycle;

    public override void _Ready()
    {
        cardPileManager.CardHovered += (Card cardUI) =>
        {
            if (cardUI is not MyCard tmp || !cardPileManager.IsCardInHand(tmp))
                return;
            descriptionLabel.Text = FormatCardDescription(tmp);
            descriptionPanel.Visible = true;
            hoveringCard = tmp;
        };

        cardPileManager.CardUnhovered += (Card cardUI) =>
        {
            descriptionPanel.Visible = false;
            hoveringCard = null;
        };

        cardPileManager.CardLeftClicked += (Card cardUI) =>
        {
            if (!cardPileManager.IsCardInHand(cardUI))
                return;
            targetingLine.SetPointPosition(0, cardUI.Position + cardUI.Size / 2);
            targetingLine.Visible = true;
        };

        cardPileManager.CardLeftReleased += (Card cardUI) =>
        {
            targetingLine.Visible = false;
        };

        endTurnButton.Pressed += OnEndButtonPressed;
        cardPileManager.DrawPileUpdated += OnDrawOrDiscardPileUpdated;
        cardPileManager.DiscardPileUpdated += OnDrawOrDiscardPileUpdated;

        cardPileManager.ResetDeck(
            ExampleDeckLoader.LoadDeck(cardDatabasePath, cardCollectionPath)
        );
        StartTurn();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (hoveringCard != null)
        {
            var targetPos = hoveringCard.Position;
            descriptionPanel.Position = targetPos + new Vector2(0, -200);
            targetingLine.SetPointPosition(1, GetGlobalMousePosition());
        }
    }

    public void OnEndButtonPressed()
    {
        EndTurn();
        StartTurn();
    }

    public void StartTurn()
    {
        Energy = TURN_ENERGY;
        Shield = 0;

        _suspendDiscardRecycle = true;
        foreach (
            var card in cardPileManager.GetCardsInPile(SimpleCardPileManager.PileKind.Hand)
        )
        {
            cardPileManager.SetCardPile(card, SimpleCardPileManager.PileKind.Discard);
        }
        _suspendDiscardRecycle = false;

        RecycleDiscardIntoDrawIfEmpty();
        cardPileManager.DrawCard(TURN_DRAW);
    }

    void OnDrawOrDiscardPileUpdated()
    {
        RecycleDiscardIntoDrawIfEmpty();
    }

    void RecycleDiscardIntoDrawIfEmpty()
    {
        if (_suspendDiscardRecycle)
            return;
        if (cardPileManager.GetCardPileSize(SimpleCardPileManager.PileKind.Draw) > 0)
            return;

        var discardCards = cardPileManager.GetCardsInPile(
            SimpleCardPileManager.PileKind.Discard
        );
        if (discardCards.Count == 0)
            return;

        foreach (var card in discardCards)
            cardPileManager.SetCardPile(card, SimpleCardPileManager.PileKind.Draw);

        var drawPile = cardPileManager.GetPile(SimpleCardPileManager.PileKind.Draw);
        drawPile.Shuffle();
        cardPileManager.UpdateCardsTargetPosition();
        cardPileManager.UpdateCardsZIndex();
    }

    public void EndTurn() { }

    public void UpdateDisplay()
    {
        energyLabel.Text = string.Format("{0}/{1}", energy, TURN_ENERGY);
        shieldLabel.Text = string.Format("{0}", shield);
    }

    public string FormatCardDescription(MyCard card)
    {
        var data = (MyCardData)card.CardData;
        return data.Description.Replace(
            "{value}",
            string.Format("[color=red]{0}[/color]", data.Value)
        );
    }
}
