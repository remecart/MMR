using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class FPSCounter : MonoBehaviour
{
    private Label _fpsLabel;

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        _fpsLabel = root.Q<Label>("FPS-Counter");

        if (_fpsLabel == null)
        {
            Debug.LogError("FPS-Counter label not found in the UI document.");
            return;
        }
    }

    private void Update()
    {
        if (_fpsLabel == null)
        {
            return;
        }

        float fps = 1.0f / Time.deltaTime;
        _fpsLabel.text = $"FPS: {fps.ToString("F2", CultureInfo.InvariantCulture)}";
    }
}