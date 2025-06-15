using UImGui;
using UnityEngine;
using VContainer;

public class SettingsUI : MonoBehaviour
{
    [Inject]
    private readonly KeybindConfig _keybindConfig;

    [Inject]
    private readonly GuiSettingsGenerator _generator;

    private bool _isActive;

    private void Awake()
    {
        UImGuiUtility.Layout += OnLayout;
    }

    private void Start()
    {
        if (_keybindConfig == null)
        {
            Debug.LogError("KeybindConfig is not injected. Please ensure it is set up in the VContainer configuration.");
        }

        _isActive = true;
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
        if (!_isActive)
        {
            return;
        }

        _generator.GenerateLayout();
    }

    private void OnDisable()
    {
        UImGuiUtility.Layout -= OnLayout;
    }
}