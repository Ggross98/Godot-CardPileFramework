namespace Ggross.CardPileFramework;

using Godot;

[GlobalClass]
public partial class CardData : Resource
{
    [Export]
    public string NiceName { get; set; }

    [Export]
    public string FrontfaceTexturePath { get; set; }

    [Export]
    public string BackfaceTexturePath { get; set; }
}
