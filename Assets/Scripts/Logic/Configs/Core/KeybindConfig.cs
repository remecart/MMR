using System.Collections.Generic;
using UnityEngine;

[ConfigFileName("Keybinds.json")]
public class KeybindConfig : IConfig
{
    public List<KeyCode> ToggleSettings { get; set; } = new List<KeyCode>();
    public List<KeyCode> StepForward { get; set; } = new List<KeyCode>();
    public List<KeyCode> StepBackwards { get; set; } = new List<KeyCode>();
}