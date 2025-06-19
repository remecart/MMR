using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using UnityEngine;
using VContainer;

public class GuiSettingsGenerator
{
    [Inject]
    private readonly ConfigLoader _configLoader;

    private void ProvideStyle()
    {
        var style = ImGui.GetStyle();

        var colors = style.Colors;
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.44f, 0.44f, 0.44f, 0.32f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.28f, 0.28f, 0.28f, 1.00f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.15f, 0.15f, 0.15f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.18f, 0.18f, 0.18f, 1.00f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.25f, 0.25f, 0.25f, 1.00f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.32f, 0.32f, 0.32f, 1.00f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.35f, 0.35f, 0.35f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.24f, 0.24f, 0.24f, 1.00f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.28f, 0.28f, 0.28f, 1.00f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);
        colors[(int)ImGuiCol.Tab] = new Vector4(0.22f, 0.22f, 0.22f, 1.00f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.28f, 0.28f, 0.28f, 1.00f);
        colors[(int)ImGuiCol.TabActive] = new Vector4(0.30f, 0.30f, 0.30f, 1.00f);

        style.WindowPadding = new Vector2(8, 8);
        style.FramePadding = new Vector2(5, 4);
        style.ItemSpacing = new Vector2(6, 4);
        style.ItemInnerSpacing = new Vector2(4, 4);
        style.ScrollbarSize = 14;
        style.GrabMinSize = 12;

        style.WindowRounding = 8;
        style.ChildRounding = 4;
        style.FrameRounding = 4;
        style.PopupRounding = 4;
        style.ScrollbarRounding = 4;
        style.GrabRounding = 4;
        style.TabRounding = 4;

        style.WindowBorderSize = 1;
        style.ChildBorderSize = 1;
        style.PopupBorderSize = 1;
        style.FrameBorderSize = 1;
    }

    public void GenerateLayout()
    {
        ProvideStyle();
        var windowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar;

        ImGui.Begin("Settings", windowFlags);

        if (!ImGui.BeginTabBar("ConfigTabs"))
        {
            return;
        }

        foreach (var config in _configLoader.GetAll())
        {
            var type = config.GetType();
            var configName = type.Name.Replace("Config", "");

            if (ImGui.BeginTabItem(configName))
            {
                var properties = type.GetProperties()
                    .Where(p => p.CanRead && p.CanWrite);

                foreach (var property in properties)
                {
                    var value = property.GetValue(config);
                    bool changed;

                    switch (value)
                    {
                        case float floatValue:
                        {
                            var rangeAttr = property.GetCustomAttribute<SliderRangeAttribute>();
                            if (rangeAttr is not null)
                            {
                                changed = RenderFloat(property.Name, ref floatValue, rangeAttr.RangeStart, rangeAttr.RangeEnd);
                            }
                            else
                            {
                                changed = RenderFloat(property.Name, ref floatValue);
                            }

                            if (changed)
                            {
                                property.SetValue(config, floatValue);
                            }

                            break;
                        }
                        case int intValue:
                        {
                            changed = RenderInt(property.Name, ref intValue);
                            if (changed)
                            {
                                property.SetValue(config, intValue);
                            }

                            break;
                        }
                        case string stringValue:
                        {
                            changed = RenderString(property.Name, ref stringValue);
                            if (changed)
                            {
                                property.SetValue(config, stringValue);
                            }

                            break;
                        }
                        case bool boolValue:
                        {
                            changed = RenderBool(property.Name, ref boolValue);
                            if (changed)
                            {
                                property.SetValue(config, boolValue);
                            }

                            break;
                        }
                        case Color colorValue:
                        {
                            changed = RenderColor(property.Name, ref colorValue);
                            if (changed)
                            {
                                property.SetValue(config, colorValue);
                            }

                            break;
                        }
                        case List<KeyCode> keyCodes:
                        {
                            changed = RenderKeybind(property.Name, keyCodes);
                            if (changed)
                            {
                                property.SetValue(config, keyCodes);
                            }

                            break;
                        }
                    }
                }

                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
        ImGui.End();
    }
    
    private bool RenderFloat(string name, ref float value, float min = 0, float max = 1)
    {
        return ImGui.SliderFloat(name, ref value, min, max);
    }

    private bool RenderInt(string name, ref int value)
    {
        return ImGui.InputInt(name, ref value);
    }

    private bool RenderString(string name, ref string value)
    {
        var buffer = value ?? string.Empty;
        if (ImGui.InputText(name, ref buffer, 256))
        {
            value = buffer;
            return true;
        }

        return false;
    }

    private bool RenderBool(string name, ref bool value)
    {
        return ImGui.Checkbox(name, ref value);
    }

    private bool RenderColor(string name, ref Color color)
    {
        var vec4 = new Vector4(color.r, color.g, color.b, color.a);
        if (ImGui.ColorEdit4(name, ref vec4))
        {
            color = new Color(vec4.x, vec4.y, vec4.z, vec4.w);
            return true;
        }

        return false;
    }

    private bool _isRecordingKeybind;

    private readonly Dictionary<string, bool> _isRecordingKeybindMap = new Dictionary<string, bool>();
    private readonly Dictionary<string, List<KeyCode>> _recordedKeysMap = new Dictionary<string, List<KeyCode>>();

    private bool RenderKeybind(string name, List<KeyCode> keyCodes)
    {
        if (!ImGui.TreeNode(name))
        {
            return false;
        }

        _isRecordingKeybindMap.TryAdd(name, false);

        if (!_recordedKeysMap.ContainsKey(name))
        {
            _recordedKeysMap[name] = new List<KeyCode>();
        }

        var changed = false;
        var keyString = string.Join(" + ", keyCodes);
        ImGui.Text($"Current: {keyString}");

        if (!_isRecordingKeybindMap[name])
        {
            if (ImGui.Button($"Record New Keybind##{name}"))
            {
                _isRecordingKeybindMap[name] = true;
                _recordedKeysMap[name].Clear();
            }

            ImGui.TreePop();
            return false;
        }

        ImGui.TextColored(new Vector4(1, 0, 0, 1), "Press keys to record (Press Enter to save, Escape to cancel)");

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key))
            {
                continue;
            }

            if (!_recordedKeysMap[name].Contains(key) && key != KeyCode.Return && key != KeyCode.Escape)
            {
                _recordedKeysMap[name].Add(key);
            }
        }

        var recordedString = string.Join(" + ", _recordedKeysMap[name]);
        ImGui.Text($"Recording: {recordedString}");

        if (Input.GetKeyDown(KeyCode.Return) && _recordedKeysMap[name].Count > 0)
        {
            keyCodes.Clear();
            keyCodes.AddRange(_recordedKeysMap[name]);
            changed = true;
            _isRecordingKeybindMap[name] = false;
            _recordedKeysMap[name].Clear();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isRecordingKeybindMap[name] = false;
            _recordedKeysMap[name].Clear();
        }

        ImGui.TreePop();
        return changed;
    }
}