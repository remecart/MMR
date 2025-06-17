using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class MapHandler : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;

    [AwakeInject] private MapLoader _mapLoader;

    [AwakeInject] private KeybindConfig _keybindConfig;

    [AwakeInject] private MapObjects _mapObjects;

    private V3Info _beatmap;

    public readonly Observable<float> CurrentBeat = new();
    [FormerlySerializedAs("spawnOffset")] public float _spawnOffset;

    private readonly Dictionary<float, ColorNote> _spawnedNotes = new();

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    private void Start()
    {
        _mapLoader.OnMapLoaded += OnMapLoaded;
        CurrentBeat.OnValueChanged += OnBeatChanged;
    }

    private void Update()
    {
        if (_keybindConfig.StepForward.Active() || Input.mouseScrollDelta.y > 0)
        {
            CurrentBeat.Value++;
        }

        if (_keybindConfig.StepBackwards.Active() || Input.mouseScrollDelta.y < 0)
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

    private void OnBeatChanged(float currentBeat)
    {
        if (_beatmap == null)
        {
            return;
        }

        var minBeat = currentBeat - _spawnOffset;
        var maxBeat = currentBeat + _spawnOffset;

        var notesToSpawn = _beatmap.ColorNotes
            .Where(x => x.Beat > minBeat && x.Beat < maxBeat && !_spawnedNotes.ContainsKey(x.Beat));
        foreach (var note in notesToSpawn)
        {
            _mapObjects.SpawnNote(note);
            _spawnedNotes[note.Beat] = note;
        }

        var beatsToDespawn = _spawnedNotes.Keys
            .Where(beat => beat <= minBeat || beat >= maxBeat)
            .ToList();

        foreach (var beat in beatsToDespawn)
        {
            _mapObjects.DespawnNote(_spawnedNotes[beat]);
            _spawnedNotes.Remove(beat);
        }
    }
}