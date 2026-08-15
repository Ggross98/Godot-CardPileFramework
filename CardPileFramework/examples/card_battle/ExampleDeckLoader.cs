using Ggross.CardPileFramework;
using Godot;
using Godot.Collections;

public static class ExampleDeckLoader
{
    public static Array<CardData> LoadDeck(string databasePath, string collectionPath)
    {
        var deck = new Array<CardData>();
        var database = ParseJson<Array<Dictionary>>(databasePath);
        var collection = ParseJson<Array<string>>(collectionPath);
        if (database == null || collection == null)
            return deck;

        var byName = new System.Collections.Generic.Dictionary<string, Dictionary>();
        foreach (var entry in database)
        {
            if (entry == null || !entry.ContainsKey("nice_name"))
                continue;
            byName[entry["nice_name"].As<string>()] = entry;
        }

        foreach (var niceName in collection)
        {
            if (!byName.TryGetValue(niceName, out var json))
                continue;
            deck.Add(CreateCardData(json));
        }

        return deck;
    }

    static T ParseJson<[MustBeVariant] T>(string path)
    {
        var text = FileAccess.GetFileAsString(path);
        if (string.IsNullOrEmpty(text))
        {
            GD.PrintErr("Cannot load json: ", path);
            return default;
        }

        return Json.ParseString(text).As<T>();
    }

    static MyCardData CreateCardData(Dictionary json)
    {
        return new MyCardData
        {
            NiceName = ReadString(json, "nice_name"),
            FrontfaceTexturePath = ReadString(json, "frontface_texture_path"),
            BackfaceTexturePath = ReadString(json, "backface_texture_path"),
            ImageTexturePath = ReadString(json, "image_texture_path"),
            Type = ReadString(json, "type"),
            Description = ReadString(json, "description"),
            Cost = ReadInt(json, "cost"),
            Value = ReadInt(json, "value"),
        };
    }

    static string ReadString(Dictionary json, string key) =>
        json.ContainsKey(key) ? json[key].As<string>() : "";

    static int ReadInt(Dictionary json, string key)
    {
        if (!json.ContainsKey(key))
            return 0;
        return (int)json[key].AsDouble();
    }
}
