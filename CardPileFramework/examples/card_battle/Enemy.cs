using Ggross.CardPileFramework;
using Godot;
using System;

public partial class Enemy : CardDropzone
{

    [Export] private Label HPLabel;
    [Export] private TextureRect portrait;

    private int hp, maxHP;
    public int HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp < 0) hp = 0;
            if (hp > maxHP) hp = maxHP;
            UpdateDisplay();
        }
    }

    public override void _Ready()
    {
        base._Ready();

        maxHP = 50;
        HP = maxHP;
    }

    protected override void OnMouseEntered()
    {
        base.OnMouseEntered();

        portrait.SelfModulate = new Color(0, 0, 0.5f, 0.5f);
    }

    protected override void OnMouseExited()
    {
        base.OnMouseExited();

        portrait.SelfModulate = Colors.White;
    }

    protected override void OnCardDropped(Card cardUi)
    {
        var data = (MyCardData)((MyCard)cardUi).cardData;
        GetNode<CardBattle>("/root/CardBattle").Energy -= data.cost;
        HP -= data.value;
        UpdateDisplay();

        var manager = (SimpleCardPileManager)base.manager;
        manager.DiscardCard(cardUi);
    }


    public override bool CanDropCard(Card cardUi)
    {

        if (!base.CanDropCard(cardUi)) return false;

        if (cardUi.GetType() == typeof(MyCard) &&

            ((MyCard)cardUi).cardData.GetType() == typeof(MyCardData))
        {
            var data = (MyCardData)((MyCard)cardUi).cardData;
            var cost = data.cost;
            var energy = GetNode<CardBattle>("/root/CardBattle").Energy;

            return data.type == "Attack" && energy >= cost;
        }
        else
        {
            return false;
        }
    }

    public void UpdateDisplay()
    {
        HPLabel.Text = string.Format("{0}/{1}", hp, maxHP);
    }
}

