using Ggross.CardPileFramework;
using Godot;

public partial class Enemy : CardDropTarget
{
    [Export]
    private Label HPLabel;

    [Export]
    private TextureRect portrait;

    private int hp,
        maxHP;
    public int HP
    {
        get => hp;
        set
        {
            hp = value;
            if (hp < 0)
                hp = 0;
            if (hp > maxHP)
                hp = maxHP;
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
        portrait.SelfModulate = new Color(0, 0, 0.5f, 0.5f);
    }

    protected override void OnMouseExited()
    {
        portrait.SelfModulate = Colors.White;
    }

    protected override void OnCardDropped(Card card)
    {
        if (card is not MyCard myCard || myCard.CardData is not MyCardData data)
            return;

        var battle = GetNode<CardBattle>("/root/CardBattle");
        if (data.Type != "Attack" || battle.Energy < data.Cost)
            return;

        battle.Energy -= data.Cost;
        HP -= data.Value;
        UpdateDisplay();

        (Manager as SimpleCardPileManager)?.DiscardCard(card);
    }

    public void UpdateDisplay()
    {
        HPLabel.Text = string.Format("{0}/{1}", hp, maxHP);
    }
}
