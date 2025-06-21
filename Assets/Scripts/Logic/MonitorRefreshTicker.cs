using System;
using UnityEngine;

public class MonitorRefreshTicker : MonoBehaviour
{
    public static event Action<float> OnMonitorTick;

    private float refreshInterval;
    private float timeAccumulator = 0f;

    private void Start()
    {
        float refreshRate = Screen.currentResolution.refreshRate;
        refreshInterval = 1f / refreshRate;
        Debug.Log($"[MonitorRefreshTicker] Using detected refresh rate: {refreshRate} Hz");
    }

    private void Update()
    {
        timeAccumulator += Time.deltaTime;

        while (timeAccumulator >= refreshInterval)
        {
            timeAccumulator -= refreshInterval;
            OnMonitorTick?.Invoke(refreshInterval);
        }
    }
}