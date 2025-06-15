using UnityEngine;

[ConfigFileName("Note-Color-Config.json")]
public class NoteColorConfig : IConfig
{
    public Color LeftColor { get; set; } = Color.red;

    public Color RightColor { get; set; } = Color.blue;
    
    public bool UseCustomColors { get; set; } = true;
}