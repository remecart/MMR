using System;
using System.Collections.Generic;
using UnityEngine;

[ConfigFileName("Audio-Config.json")]
public class AudioConfig : IConfig
{
    public float SongVolume { get; set; } = 15f;
    public float HitsoundVolume { get; set; } = 2f;
}