using System;
using UnityEngine;

public class MapPlacementHandler : MonoBehaviour
{
    [SerializeField]
    private MeshCollider _meshCollider;

    private void Start()
    {
        MonitorRefreshTicker.OnMonitorTick += IsCollidingWithMesh;
    }

    private void IsCollidingWithMesh(float delta)
    {
        if (Camera.main == null)
        {
            Debug.LogError("Camera missing");
            return;
        }

        // Mouse Raycast to check if collding with meshCollider
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!_meshCollider.Raycast(ray, out var hit, 100.0f))
        {
            return;
        }

        var point = ray.GetPoint(100.0f);
        var rounded = (Mathf.RoundToInt(point.x * 2) / 2) / 10;
        Debug.Log(rounded);
    }
}