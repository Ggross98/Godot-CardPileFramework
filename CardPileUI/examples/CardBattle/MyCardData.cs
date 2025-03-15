using Godot;
using System;

public partial class MyCardData : CardUIData
{
    [Export]
    public int cost, value;

    [Export]
    public string type, description;
}
