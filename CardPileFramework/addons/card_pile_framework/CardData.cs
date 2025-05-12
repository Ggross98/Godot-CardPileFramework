namespace Ggross.CardPileFramework;

using System;
using Godot;
using Godot.Collections;

public partial class CardData : Resource
{
    [Export]
    public string nice_name;

    [Export]
    public string frontface_texture_path,
        backface_texture_path,
        resource_script_path;

    public void LoadProperties(Dictionary jsonData)
    {
        foreach (var k in jsonData.Keys)
        {
            var key = k.As<string>();
            Set(key, jsonData[key]);
            // GD.Print(key, ": ", jsonData[key]);
        }
    }
}
