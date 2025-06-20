using System;
using System.Collections.Generic;
using System.Linq;
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
    
    public void CreateBeatLines(float currentBeat, float editorScale, float spawnOffset)
    {
        QuarterBeatLines(currentBeat, editorScale, spawnOffset);
        FullBeatLines(currentBeat, editorScale, spawnOffset);
    }

    private void FullBeatLines(float currentBeat, float editorScale, float spawnOffset)
    {
        var minBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) - spawnOffset);
        var maxBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) + spawnOffset);

        minBeat = Math.Clamp(minBeat, 0, 99999);
            
        var fullBeats = Mathf.FloorToInt(maxBeat) - Mathf.CeilToInt(minBeat);

        for (var i = 0; i < fullBeats + 1; i++)
        {
            var beat = Mathf.CeilToInt(minBeat) + i;
            
            if (LineCache.FirstOrDefault(go => go.name == beat.ToString()))
            {
                continue; // Schon gespawnt
            }

            var go = Instantiate(BeatLine, this.transform, false);
            go.transform.localPosition = new Vector3(0, 0,
                _bpmConverter.GetPositionFromBeat(beat) * editorScale);
            go.name = $"{beat}";
            
            LineCache.Add(go);
        }
        
        
        var linesToDespawn = new List<GameObject>();

        for (var index = 0; index < LineCache.Count; index++)
        {
            var line = LineCache[index];
            if (float.Parse(line.name) > maxBeat ||
                float.Parse(line.name) < minBeat)
            {
                linesToDespawn.Add(line);
            }
        }

        foreach (var line in linesToDespawn)
        {
            LineCache.Remove(line);
            Destroy(line);
        }
        
        linesToDespawn.Clear();
    }
    
    private void QuarterBeatLines(float currentBeat, float editorScale, float spawnOffset)
    {
        var minBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) - spawnOffset);
        var maxBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) + spawnOffset);

        minBeat = Math.Clamp(minBeat, 0, 99999);

        // Iterate in quarter beat steps
        for (float beat = Mathf.Ceil(minBeat * 4) / 4f; beat <= maxBeat; beat += 0.25f)
        {
            var beatKey = beat.ToString("F2");

            if (LineCache.Any(go => go.name == beatKey))
            {
                continue; // Schon gespawnt
            }

            if (Mathf.Approximately(beat, Mathf.RoundToInt(beat)))
            {
                continue; // Voller beat
            }

            var go = Instantiate(SubBeatLine, this.transform, false);
            go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat) * editorScale);
            go.name = beatKey;

            LineCache.Add(go);
        }

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
