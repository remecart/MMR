using System;
using System.Collections.Generic;
using UnityEngine;

[ConfigFileName("MBot-Config.json")]
public class MBotConfig : IConfig
{
    [SliderRange(0.5f, 3f)] 
    public float Intensity { get; set; } = 2f;
    [SliderRange(0.5f, 3f)] 
    public float Overshoot { get; set; } = 1.5f;
    [SliderRange(0.5f, 3f)] 
    public float PositionMultiplier { get; set; } = 1.5f;
    [SliderRange(0f, 2f)] 
    public float PlaneOffset { get; set; } = 2f;
}