using System;
using Newtonsoft.Json;
using UnityEngine;

public class KeyCodeConverter : JsonConverter<KeyCode>
{
    public override void WriteJson(JsonWriter writer, KeyCode value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override KeyCode ReadJson(JsonReader reader, Type objectType, KeyCode existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var keyName = (string)reader.Value;
        return Enum.TryParse<KeyCode>(keyName, out var result) ? result : KeyCode.None;
    }
}
