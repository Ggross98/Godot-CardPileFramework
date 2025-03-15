using Godot;
using Godot.Collections;
using System;

public partial class CardUIData: Resource
{
    [Export] public string nice_name;
    [Export] public string frontface_texture_path, backface_texture_path, resource_script_path;

    public void LoadProperties(Dictionary jsonData){
        foreach (var k in jsonData.Keys)
        {   
            var key = k.As<string>();
            Set(key, jsonData[key]);
            // GD.Print(key, ": ", jsonData[key]);
        }
    }

}
