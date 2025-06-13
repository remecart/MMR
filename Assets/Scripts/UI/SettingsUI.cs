using System.Collections;
using System.Collections.Generic;
using ImGuiNET;
using UImGui;
using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    private void Awake()
    {
        UImGuiUtility.Layout += OnLayout;
        UImGuiUtility.OnInitialize += OnInitialize;
        UImGuiUtility.OnDeinitialize += OnDeinitialize;
    }

    private void OnLayout(UImGui.UImGui obj)
    {
        // Unity Update method. 
        // Your code belongs here! Like ImGui.Begin... etc.

        if (ImGui.Begin("Settings"))
        {
            ImGui.Text("This is a settings window.");
            if (ImGui.Button("Close"))
            {
                ImGui.CloseCurrentPopup();
            }
        }
    }

    private void OnInitialize(UImGui.UImGui obj)
    {
        // runs after UImGui.OnEnable();
    }

    private void OnDeinitialize(UImGui.UImGui obj)
    {
        // runs after UImGui.OnDisable();
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
        UImGuiUtility.OnInitialize -= OnInitialize;
        UImGuiUtility.OnDeinitialize -= OnDeinitialize;
    }
}