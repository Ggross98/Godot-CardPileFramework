using Ggross.CardPileFramework;
using Godot;
using System;

public partial class MyCardData : CardData
{
    [Export]
    public int cost, value;

    [Export]
    public string type, description, image_texture_path;
}
