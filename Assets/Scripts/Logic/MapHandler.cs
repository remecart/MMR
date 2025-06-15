using System;
using System.Linq;
using UnityEngine;
using VContainer;

public class MapHandler : MonoBehaviour
{
    [Inject]
    private readonly MapLoader _mapLoader;
    
    [Inject]
    private readonly KeybindConfig _keybindConfig;

    private V3Info _beatmap;
    
    public Observable<float> CurrentBeat = new();
    public float spawnOffset;

    private void Start()
    {
        _mapLoader.OnMapLoaded += OnMapLoaded;
        CurrentBeat.OnValueChanged += SpawnMapObjects;
    }

    private void Update()
    {
        if (_keybindConfig.StepForward.Active())
        {
            CurrentBeat.Value++;
        }
        
        if (_keybindConfig.StepBackwards.Active())
        {
            CurrentBeat.Value--;
        }
    }

    private void OnMapLoaded(V3Info obj)
    {
        _beatmap = obj;
    }

    private void OnDisable()
    {
        _mapLoader.OnMapLoaded -= OnMapLoaded;
    }

    private void SpawnMapObjects(float currentBeat)
    {
        var notes = _beatmap.ColorNotes.Where(x =>
            x.Beat > currentBeat - spawnOffset && x.Beat < currentBeat + spawnOffset);

        foreach (var note in notes)
        {
            // var go = Instantiate();
        }
    }
}