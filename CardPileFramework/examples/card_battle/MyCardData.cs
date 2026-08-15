using Ggross.CardPileFramework;
using Godot;

public partial class MyCardData : CardData
{
    [Export]
    public int Cost { get; set; }

    [Export]
    public int Value { get; set; }

    [Export]
    public string Type { get; set; }

    [Export]
    public string Description { get; set; }

    [Export]
    public string ImageTexturePath { get; set; }
}
