using System.Collections.Generic;
using UnityEngine;

[ConfigFileName("Keybinds.json")]
public class KeybindConfig : IConfig
{
    public List<KeyCode> ToggleSettings { get; set; } = new List<KeyCode>
    {
        KeyCode.Tab,
    };
}