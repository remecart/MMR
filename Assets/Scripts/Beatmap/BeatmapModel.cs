using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;


[Serializable]
public class V3Info
{
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("bpmEvents", NullValueHandling = NullValueHandling.Ignore)]
    public List<BpmEvent> BpmEvents { get; set; } = new();

    [JsonProperty("rotationEvents", NullValueHandling = NullValueHandling.Ignore)]
    public List<RotationEvent> RotationEvents { get; set; } = new();

    [JsonProperty("colorNotes", NullValueHandling = NullValueHandling.Ignore)]
    public List<ColorNote> ColorNotes { get; set; } = new();

    [JsonProperty("bombNotes", NullValueHandling = NullValueHandling.Ignore)]
    public List<BombNote> BombNotes { get; set; } = new();

    [JsonProperty("obstacles", NullValueHandling = NullValueHandling.Ignore)]
    public List<Obstacle> Obstacles { get; set; } = new();

    [JsonProperty("sliders", NullValueHandling = NullValueHandling.Ignore)]
    public List<Slider> Sliders { get; set; } = new();

    [JsonProperty("burstSliders", NullValueHandling = NullValueHandling.Ignore)]
    public List<BurstSlider> BurstSliders { get; set; } = new();

    [JsonProperty("basicBeatmapEvents", NullValueHandling = NullValueHandling.Ignore)]
    public List<BasicBeatmapEvent> BasicBeatmapEvents { get; set; } = new();

    [JsonProperty("timings", NullValueHandling = NullValueHandling.Ignore)]
    public List<Timing> Timings { get; set; } = new();
}

[Serializable]
public abstract class BeatmapObject
{
    [JsonProperty("b")]
    public float Beat { get; set; }

    [JsonProperty("x", NullValueHandling = NullValueHandling.Ignore)]
    public int? X { get; set; }

    [JsonProperty("y", NullValueHandling = NullValueHandling.Ignore)]
    public int? Y { get; set; }
}

[Serializable]
public class BpmEvent : BeatmapObject
{
    [JsonProperty("m")]
    public float Multiplier { get; set; }
}

[Serializable]
public class RotationEvent : BeatmapObject
{
    [JsonProperty("e")]
    public int EventType { get; set; }

    [JsonProperty("r")]
    public int Rotation { get; set; }
}

[Serializable]
public class ColorNote : BeatmapObject
{
    [JsonProperty("a")]
    public int Angle { get; set; }

    [JsonProperty("c")]
    public SaberType SaberType { get; set; }

    [JsonProperty("d")]
    public int Direction { get; set; }
    
    [JsonProperty("customData", NullValueHandling = NullValueHandling.Ignore)]
    public NoteCustomData CustomData { get; set; }
}

[Serializable]
public class BombNote : BeatmapObject
{
    [JsonProperty("customData", NullValueHandling = NullValueHandling.Ignore)]
    public NoteCustomData CustomData { get; set; }
}

[Serializable]
public class Obstacle : BeatmapObject
{
    [JsonProperty("d")]
    public float Duration { get; set; }

    [JsonProperty("h")]
    public int Height { get; set; }

    [JsonProperty("w")]
    public int Width { get; set; }
    
    [JsonProperty("customData", NullValueHandling = NullValueHandling.Ignore)]
    public ObstacleCustomData CustomData { get; set; }
}

[Serializable]
public class Slider : BeatmapObject
{
    [JsonProperty("c")]
    public SaberType SaberType { get; set; }

    [JsonProperty("d")]
    public int Direction { get; set; }

    [JsonProperty("mu")]
    public float Multiplier { get; set; }

    [JsonProperty("tb")]
    public float TailBeat { get; set; }

    [JsonProperty("tx")]
    public int TailX { get; set; }

    [JsonProperty("ty")]
    public int TailY { get; set; }

    [JsonProperty("tc")]
    public int TailColor { get; set; }

    [JsonProperty("tmu")]
    public float TailMultiplier { get; set; }

    [JsonProperty("m")]
    public int Mode { get; set; }
    
    [JsonProperty("customData", NullValueHandling = NullValueHandling.Ignore)]
    public SliderCustomData CustomData { get; set; }
}

[Serializable]
public class BurstSlider : Slider
{
    [JsonProperty("sc")]
    public int SliceCount { get; set; }

    [JsonProperty("s")]
    public int Squish { get; set; }
}

[Serializable]
public class BasicBeatmapEvent : BeatmapObject
{
    [JsonProperty("et")]
    public int EventType { get; set; }

    [JsonProperty("i")]
    public int Value { get; set; }

    [JsonProperty("f")]
    public float FloatValue { get; set; }
}

[Serializable]
public class Timing : BeatmapObject
{
    [JsonProperty("t")]
    public int Type { get; set; }
}


[Serializable]
public class CustomData
{
    [JsonProperty("track", NullValueHandling = NullValueHandling.Ignore)]
    public string Track { get; set; }

    [JsonProperty("coordinates", NullValueHandling = NullValueHandling.Ignore)]
    public Vector2? Coordinates { get; set; } // Nullable Vector2

    [JsonProperty("worldRotation", NullValueHandling = NullValueHandling.Ignore)]
    public Vector3? WorldRotation { get; set; } // Nullable Vector3

    [JsonProperty("localRotation", NullValueHandling = NullValueHandling.Ignore)]
    public Vector3? LocalRotation { get; set; } // Nullable Vector3

    [JsonProperty("scale", NullValueHandling = NullValueHandling.Ignore)]
    public Vector3? Scale { get; set; } // Nullable Vector3

    [JsonProperty("noteJumpMovementSpeed", NullValueHandling = NullValueHandling.Ignore)]
    public float? NoteJumpMovementSpeed { get; set; } // Nullable float

    [JsonProperty("noteJumpStartBeatOffset", NullValueHandling = NullValueHandling.Ignore)]
    public float? NoteJumpStartBeatOffset { get; set; } // Nullable float

    [JsonProperty("uninteractable", NullValueHandling = NullValueHandling.Ignore)]
    public bool? Uninteractable { get; set; } // Nullable bool

    [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
    public Color? Color { get; set; } // Nullable Color

    [JsonProperty("animation", NullValueHandling = NullValueHandling.Ignore)]
    public AnimationData Animation { get; set; }
}

[Serializable]
public class NoteCustomData : CustomData
{
    [JsonProperty("flip", NullValueHandling = NullValueHandling.Ignore)]
    public bool Flip { get; set; }

    [JsonProperty("disableNoteGravity", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableNoteGravity { get; set; } // Nullable bool

    [JsonProperty("disableNoteLook", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableNoteLook { get; set; } // Nullable bool

    [JsonProperty("disableBadCutDirection", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableBadCutDirection { get; set; } // Nullable bool

    [JsonProperty("disableBadCutSpeed", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableBadCutSpeed { get; set; } // Nullable bool

    [JsonProperty("disableBadCutSaberType", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableBadCutSaberType { get; set; } // Nullable bool

    [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
    public string Link { get; set; }

    [JsonProperty("spawnEffect", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableSpawnEffect { get; set; } // Nullable bool

    [JsonProperty("disableDebris", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableDebris { get; set; } // Nullable bool
}

[Serializable]
public class ObstacleCustomData : CustomData
{
    [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
    public Vector3? Size { get; set; } // Nullable Vector3
}

[Serializable]
public class SliderCustomData : CustomData
{
    [JsonProperty("disableNoteGravity", NullValueHandling = NullValueHandling.Ignore)]
    public bool? DisableNoteGravity { get; set; } // Nullable bool

    [JsonProperty("tailCoordinates", NullValueHandling = NullValueHandling.Ignore)]
    public Vector2? TailCoordinates { get; set; } // Nullable Vector2
}


[Serializable]
public class Bookmark
{
    [JsonProperty("b")]
    public float Beat { get; set; }

    [JsonProperty("n")]
    public string Name { get; set; }

    [JsonProperty("c")]
    public List<float> Color { get; set; } = new();
}

public enum SaberType
{
    Left = 0,
    Right = 1
}