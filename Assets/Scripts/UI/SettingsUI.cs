using ImGuiNET;
using UImGui;
using UnityEngine;
using VContainer;

public class SettingsUI : MonoBehaviour
{
    [Inject]
    private readonly KeybindConfig _keybindConfig;

    private bool _isActive;

    private void Awake()
    {
        UImGuiUtility.Layout += OnLayout;
        UImGuiUtility.OnInitialize += OnInitialize;
        UImGuiUtility.OnDeinitialize += OnDeinitialize;
    }

    private void Start()
    {
        if (_keybindConfig == null)
        {
            Debug.LogError("KeybindConfig is not injected. Please ensure it is set up in the VContainer configuration.");
        }
    }

    private void Update()
    {
        if (_keybindConfig
            .ToggleSettings
            .Active())
        {
            _isActive = !_isActive;
        }
    }

    private void OnLayout(UImGui.UImGui obj)
    {
        // Unity Update method. 
        // Your code belongs here! Like ImGui.Begin... etc.
        if (!_isActive)
        {
            return;
        }

        if (ImGui.Begin("Settings"))
        {
            ImGui.Text("This is a settings window.");
            if (ImGui.Button("Close"))
            {
                _isActive = false;
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