using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


[Serializable]
public class AnimationData
{
    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationColorKeyframe> Colors { get; set; } = new();

    [JsonProperty("offsetPosition", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationOffsetKeyframe> OffsetPositions { get; set; } = new();

    [JsonProperty("localRotation", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationLocalRotationKeyframe> LocalRotations { get; set; } = new();

    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationScaleKeyframe> Scales { get; set; } = new();

    [JsonProperty("dissolve", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationDissolveKeyframe> Dissolves { get; set; } = new();

    [JsonProperty("dissolveArrow", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationDissolveArrowKeyframe> DissolveArrows { get; set; } = new();

    [JsonProperty("interactable", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationInteractableKeyframe> Interactables { get; set; } = new();

    [JsonProperty("definitePosition", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationDefinitePositionKeyframe> DefinitePositions { get; set; } = new();

    [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
    public List<AnimationTimeKeyframe> Times { get; set; } = new();


    public override string ToString()
    {
        return
            $"Colors: {Colors.Count}, OffsetPositions: {OffsetPositions.Count}, LocalRotations: {LocalRotations.Count}, Scales: {Scales.Count}, Dissolves: {Dissolves.Count}, DissolveArrows: {DissolveArrows.Count}, Interactables: {Interactables.Count}, DefinitePositions: {DefinitePositions.Count}, Times: {Times.Count}";
    }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationOffsetKeyframe>))]
public class AnimationOffsetKeyframe : Animatable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string Spline { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationLocalRotationKeyframe>))]
public class AnimationLocalRotationKeyframe : Animatable
{
    public float Pitch { get; set; }
    public float Yaw { get; set; }
    public float Roll { get; set; }
    public string Spline { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationScaleKeyframe>))]
public class AnimationScaleKeyframe : Animatable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public string Spline { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationDissolveKeyframe>))]
public class AnimationDissolveKeyframe : Animatable
{
    public float Transparency { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationDissolveArrowKeyframe>))]
public class AnimationDissolveArrowKeyframe : Animatable
{
    public float Transparency { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationDefinitePositionKeyframe>))]
public class AnimationDefinitePositionKeyframe : Animatable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationInteractableKeyframe>))]
public class AnimationInteractableKeyframe : Animatable
{
    public bool Interactable { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationTimeKeyframe>))]
public class AnimationTimeKeyframe : Animatable
{
    public float ObjectTime { get; set; }
}

[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<AnimationColorKeyframe>))]
public class AnimationColorKeyframe : Animatable
{
    public float Red { get; set; }
    public float Green { get; set; }
    public float Blue { get; set; }
    public float Alpha { get; set; }
}


[Serializable]
[JsonConverter(typeof(BaseAnimationConverter<Animatable>))]
public class Animatable
{
    public float Time { get; set; }
    [CanBeNull]
    public string Easing { get; set; }
}


public class BaseAnimationConverter<T> : JsonConverter<T> where T : Animatable, new()
{
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        PropertyInfo[] properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();

        T obj = new T();
        int count = Math.Min(properties.Length, array.Count);

        for (int i = 0; i < count; i++)
        {
            object value = array[i].ToObject(properties[i].PropertyType, serializer);
            properties[i].SetValue(obj, value);
        }

        return obj;
    }

    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        PropertyInfo[] properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .ToArray();

        JArray array = new JArray();

        foreach (var prop in properties)
        {
            object propValue = prop.GetValue(value);
            if (propValue != null) array.Add(JToken.FromObject(propValue, serializer));
        }

        array.WriteTo(writer);
    }
}