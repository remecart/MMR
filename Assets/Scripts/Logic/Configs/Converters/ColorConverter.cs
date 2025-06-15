using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        JObject obj = new()
        {
            { "r", value.r },
            { "g", value.g },
            { "b", value.b },
            { "a", value.a }
        };
        obj.WriteTo(writer);
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Color(
            (float)obj["r"],
            (float)obj["g"],
            (float)obj["b"],
            (float)obj["a"]
        );
    }
}