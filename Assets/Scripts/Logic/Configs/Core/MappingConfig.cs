using System;
using System.Collections.Generic;
using UnityEngine;

[ConfigFileName("Mapping-Config.json")]
public class MappingConfig : IConfig
{
    [SliderRange(5f, 25f)]
    public float EditorScale { get; set; } = 15f;
    [SliderRange(1f, 5f)]
    public float SpawnOffset { get; set; } = 2f;
}

public class SliderRangeAttribute : Attribute
{
    public SliderRangeAttribute(float f, float f1)
    {
        RangeStart = f;
        RangeEnd = f1;
    }
    
    public float RangeStart { get; set; }
    public float RangeEnd { get; set; }
}