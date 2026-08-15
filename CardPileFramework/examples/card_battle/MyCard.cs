using Ggross.CardPileFramework;
using Godot;

public partial class MyCard : Card
{
    [Export]
    private Label nameLabel,
        typeLabel,
        costLabel;

    [Export]
    private TextureRect image;

    public override void UpdateDisplay()
    {
        base.UpdateDisplay();

        if (cardData is not MyCardData data)
            return;

        nameLabel.Text = data.NiceName;
        typeLabel.Text = data.Type;
        costLabel.Text = data.Cost.ToString();
        image.Texture = GD.Load<Texture2D>(data.ImageTexturePath);
    }
}
