using System;
using System.Collections.Generic;
using UnityEngine;

[ConfigFileName("Audio-Config.json")]
public class AudioConfig : IConfig
{
    [SliderRange(0f, 1f)]
    public float SongVolume { get; set; } = 0.666f;
    [SliderRange(0f, 1f)]
    public float HitsoundVolume { get; set; } = 0.5f;
}