using System;
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

    public const int TURN_ENERGY = 4,
        MAX_HP = 50,
        TURN_DRAW = 5;
    private int energy,
        shield,
        hp;
    public int Energy
    {
        get { return energy; }
        set
        {
            energy = value;
            UpdateDisplay();
        }
    }
    public int Shield
    {
        get { return shield; }
        set
        {
            shield = value;
            UpdateDisplay();
        }
    }
    public int HP
    {
        get { return hp; }
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

    public override void _Ready()
    {
        cardPileManager.CardHovered += (Card cardUI) =>
        {
            var tmp = (MyCard)cardUI;
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
            targetingLine.SetPointPosition(0, cardUI.Position + cardUI.Size / 2);
            targetingLine.Visible = true;
        };

        cardPileManager.CardLeftReleased += (Card cardUI) =>
        {
            targetingLine.Visible = false;
        };

        endTurnButton.Pressed += OnEndButtonPressed;

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
        foreach (
            var card in cardPileManager.GetCardsInPile(SimpleCardPileManager.DropzoneType.HandPile)
        )
        {
            cardPileManager.SetCardPile(card, SimpleCardPileManager.DropzoneType.DiscardPile);
        }

        cardPileManager.DrawCard(TURN_DRAW);
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
        return data.description.Replace(
            "{value}",
            string.Format("[color=red]{0}[/color]", data.value)
        );
    }
}
