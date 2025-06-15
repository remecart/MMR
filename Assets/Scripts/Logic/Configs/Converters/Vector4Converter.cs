using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class Vector4Converter : JsonConverter<Vector4>
{
    public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
    {
        JObject obj = new()
        {
            { "x", value.x },
            { "y", value.y },
            { "z", value.z },
            { "w", value.w }
        };
        obj.WriteTo(writer);
    }

    public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        return new Vector4((float)obj["x"], (float)obj["y"], (float)obj["z"], (float)obj["w"]);
    }
}