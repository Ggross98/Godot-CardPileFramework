using Godot;
using Godot.Collections;
using System;

public class JsonUtils
{
    public static T LoadJsonAs<[MustBeVariant]T>(string path){

        T ret = default;

        try{
            var jsonAsText = FileAccess.GetFileAsString(path);
            ret = Json.ParseString(jsonAsText).As<T>();
        }
        catch(Exception e){
            GD.Print("Cannot load the json file. " + e.Message);
        }
        
        return ret;
    }
}
