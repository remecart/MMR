using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapHandler : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;

    [AwakeInject] private ConfigLoader _configLoader;

    [AwakeInject] private MapLoader _mapLoader;

    [AwakeInject] private KeybindConfig _keybindConfig;
    
    [AwakeInject] private readonly MappingConfig _mappingConfig;

    [AwakeInject] private MapObjects _mapObjects;
    
    [AwakeInject] private readonly BpmConverter _bpmConverter;

    private V3Info _beatmap;

    public readonly Observable<float> CurrentBeat = new();
    public float _spawnOffset => _mappingConfig.SpawnOffset;

    private readonly List<ColorNote> _spawnedNotes = new();
    public bool isPlaying;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    private void Start()
    {
        _mapLoader.OnMapLoaded += OnMapLoaded;
        
        CurrentBeat.OnValueChanged += OnBeatChanged;

        MonitorRefreshTicker.OnMonitorTick += PlayMap;
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

        if (_keybindConfig.TogglePlaymode.Active()) isPlaying = !isPlaying;

    }

    private void PlayMap(float refreshInterval)
    {
        if (isPlaying)
        {
            // CurrentBeat.Value += _bpmConverter.GetBpmAtBeat(CurrentBeat) / 60f * refreshInterval;

            var currentTime = _bpmConverter.GetRealTimeFromBeat(CurrentBeat.Value);
            var increasedTime = currentTime + refreshInterval;
            var updatedBeat = _bpmConverter.GetBeatFromRealTime(increasedTime);

            CurrentBeat.Value = updatedBeat;
        }
    }

    private void FixedUpdate()
    {
        foreach (var config in _configLoader.GetAll())
        {
            var type = config.GetType();
            if (_configLoader.IsChanged(type))
            { 
                ReloadBeat();
            }
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

    public void ReloadBeat()
    {
        _spawnedNotes.Clear();
        _mapObjects.Notes.Clear();
        
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        OnBeatChanged(CurrentBeat.Value);
    }
    
    private void OnBeatChanged(float currentBeat)
    {
        if (_beatmap == null)
            return;

        transform.localPosition = new Vector3(0, 0, - _bpmConverter.GetPositionFromBeat(currentBeat) * _mappingConfig.EditorScale);

        float minBeat = currentBeat - _spawnOffset;
        float maxBeat = currentBeat + _spawnOffset;

        // Despawn notes out of range
        var notesToDespawn = _spawnedNotes
            .Where(note => note.Beat <= minBeat || note.Beat >= maxBeat)
            .ToList();

        foreach (var note in notesToDespawn)
        {
            _mapObjects.DespawnNote(note);
            _spawnedNotes.Remove(note);
        }

        // Spawn notes in range that aren't already spawned
        var notesToSpawn = _beatmap.ColorNotes
            .Where(note => note.Beat >= minBeat && note.Beat < maxBeat && !_spawnedNotes.Contains(note));

        foreach (var note in notesToSpawn)
        {
            _mapObjects.SpawnNote(note);
            _spawnedNotes.Add(note);
        }
        
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<ColorNoteObject>().SetTransparent();
        }
    }
}