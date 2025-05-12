using System;
using Ggross.CardPileFramework;
using Godot;

public partial class MyCardData : CardData
{
    [Export]
    public int cost,
        value;

    [Export]
    public string type,
        description,
        image_texture_path;
}
