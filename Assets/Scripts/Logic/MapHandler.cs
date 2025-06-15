using System;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapHandler : MonoBehaviour
{
    [Inject]
    private readonly LifetimeScope scope;

    [AwakeInject]
    private MapLoader _mapLoader;

    [AwakeInject]
    private KeybindConfig _keybindConfig;

    [AwakeInject]
    private MapObjects _mapObjects;

    private V3Info _beatmap;

    public Observable<float> CurrentBeat = new Observable<float>();
    public float spawnOffset;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, scope);
    }

    private void Start()
    {
        _mapLoader.OnMapLoaded += OnMapLoaded;
        CurrentBeat.OnValueChanged += SpawnMapObjects;

        Debug.Log(_mapObjects == null);
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