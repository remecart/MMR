using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BeatLines : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;
    
    [AwakeInject] private BpmConverter _bpmConverter;
    
    public GameObject BeatLine;
    public GameObject SubBeatLine;
    public GameObject GuideLine;
    
    public int precision;

    public List<GameObject> LineCache;
    
    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    public void ClearLines()
    {
        LineCache.Clear();
        
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void SpawnBeatLines(float currentBeat, float editorScale, float spawnOffset) // precision >= 1 
    {
        var minBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) - spawnOffset);
        var maxBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) + spawnOffset);

        minBeat = Math.Clamp(minBeat, 0, 9999);

        // Iterate in quarter beat steps
        for (float beat = Mathf.Ceil(minBeat * precision) / precision; beat <= maxBeat; beat += 1f / precision)
        {
            if (LineCache.Any(go =>
                    float.TryParse(go.name, out var parsed) &&
                    Mathf.Approximately(parsed, beat)))
            {
                continue; // Already spawned
            }

            if (Mathf.Abs(Mathf.Round(beat) - beat) < 0.01f)
            {
                var go = Instantiate(BeatLine, transform.GetChild(0).transform, false);
                go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat) * editorScale);
                go.name = $"{beat}";
                go.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = Mathf.Round(beat).ToString(CultureInfo.InvariantCulture);
                LineCache.Add(go);

            }
            else
            {
                var go = Instantiate(SubBeatLine, transform.GetChild(0).transform, false);
                go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat) * editorScale);
                go.name = $"{beat}";
                LineCache.Add(go);
            }
        }

        SpawnGuideLines(currentBeat, editorScale, spawnOffset, minBeat, maxBeat);
        DespawnBeatLines(minBeat, maxBeat);
    }

    private List<GameObject> _guideLinePool = new();

    private void SpawnGuideLines(float currentBeat, float editorScale, float spawnOffset, float minBeat, float maxBeat)
    {
        var minZ = _bpmConverter.GetPositionFromBeat(minBeat) * editorScale;
        var maxZ = _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale;
        var centerZ = (minZ + maxZ) / 2f;
        var lengthZ = Mathf.Abs(maxZ - minZ);

        for (var i = 0; i < 5; i++)
        {
            float x = i - 2;

            GameObject line;
            if (i < _guideLinePool.Count && _guideLinePool[i] != null)
            {
                line = _guideLinePool[i];
                line.SetActive(true);
            }
            else
            {
                line = Instantiate(GuideLine, transform.GetChild(1).transform, false);
                if (i >= _guideLinePool.Count)
                    _guideLinePool.Add(line);
                else
                    _guideLinePool[i] = line;
            }

            line.transform.localPosition = new Vector3(x, 0, centerZ);
            line.transform.localScale = new Vector3(0.02f, 0.01f, lengthZ);
        }

        // Deaktiviere alle überschüssigen Linien, falls z. B. Pool vorher länger war
        for (var i = 5; i < _guideLinePool.Count; i++)
        {
            if (_guideLinePool[i] != null)
            {
                _guideLinePool[i].SetActive(false);
            }
        }
    }
    
    private void DespawnBeatLines(float minBeat, float maxBeat)
    {
        // Clean up out-of-range lines
        var linesToDespawn = LineCache
            .Where(line =>
            {
                if (float.TryParse(line.name, out var parsed))
                {
                    return parsed < minBeat || parsed > maxBeat;
                }

                return false;
            })
            .ToList();

        foreach (var line in linesToDespawn)
        {
            LineCache.Remove(line);
            Destroy(line);
        }
    }
}
