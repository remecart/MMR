using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapHandler : MonoBehaviour
{
    [Inject]
    private readonly LifetimeScope _scope;

    [AwakeInject]
    private ConfigLoader _configLoader;

    [AwakeInject]
    private MapLoader _mapLoader;

    [AwakeInject]
    private KeybindConfig _keybindConfig;

    [AwakeInject]
    private readonly MappingConfig _mappingConfig;

    [AwakeInject]
    private readonly SongLoader _songLoader;

    [AwakeInject]
    private MapObjects _mapObjects;

    [AwakeInject]
    private readonly BpmConverter _bpmConverter;

    public BeatLines BeatLines;

    private V3Info _beatmap;

    public readonly Observable<float> CurrentBeat = new();
    public int precision;

    private float EditorScale => _mappingConfig.EditorScale;

    private float SpawnOffset => _mappingConfig.SpawnOffset;

    private readonly List<ColorNote> _spawnedNotes = new();
    private readonly List<BombNote> _spawnedBombs = new();
    private readonly List<Obstacle> _spawnedObstacles = new();

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
            if (Input.GetKey(KeyCode.LeftControl))
            {
                precision = Mathf.RoundToInt(precision / 2);
                if (precision < 3)
                {
                    precision = 4;
                }

                BeatLines.SpawnBeatLines(CurrentBeat.Value, EditorScale, SpawnOffset, precision);
                // Log spawning beat lines with precision
                Debug.Log($"Spawning beat lines at beat {CurrentBeat.Value} with precision {precision}");
            }
            else if (!isPlaying)
            {
                CurrentBeat.Value++;
            }
        }

        if (_keybindConfig.StepBackwards.Active() || Input.mouseScrollDelta.y < 0 && !Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                precision = Mathf.RoundToInt(precision * 2);
                if (precision > 64)
                {
                    precision = 64;
                }

                BeatLines.SpawnBeatLines(CurrentBeat.Value, EditorScale, SpawnOffset, precision);
            }
            else if (!isPlaying)
            {
                if (CurrentBeat.Value - 1 >= 0)
                {
                    CurrentBeat.Value--;
                }
            }
        }

        if (_keybindConfig.TogglePlaymode.Active())
        {
            isPlaying = !isPlaying;

            if (isPlaying)
            {
                _songLoader.PlaySong(_bpmConverter.GetRealTimeFromBeat(CurrentBeat.Value));
            }
            else
            {
                _songLoader.StopSong();
            }
        }
    }

    private void PlayMap(float refreshInterval)
    {
        if (!isPlaying)
        {
            return;
        }
        // CurrentBeat.Value += _bpmConverter.GetBpmAtBeat(CurrentBeat) / 60f * refreshInterval;

        var currentTime = _bpmConverter.GetRealTimeFromBeat(CurrentBeat.Value);
        var increasedTime = currentTime + refreshInterval;
        var updatedBeat = _bpmConverter.GetBeatFromRealTime(increasedTime);

        CurrentBeat.Value = updatedBeat;
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
        _spawnedBombs.Clear();
        _spawnedObstacles.Clear();

        _mapObjects.Notes.Clear();
        _mapObjects.Bombs.Clear();
        _mapObjects.Obstacles.Clear();

        transform.parent.localPosition = new Vector3(0, 0, -_bpmConverter.GetPositionFromBeat(CurrentBeat.Value) * _mappingConfig.EditorScale);

        BeatLines.ResetLines();

        BeatLines.SpawnBeatLines(CurrentBeat.Value, EditorScale, SpawnOffset, precision);

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        OnBeatChanged(CurrentBeat.Value);
    }

    private void OnBeatChanged(float currentBeat)
    {
        var editorScale = _mappingConfig.EditorScale;
        var spawnOffset = _mappingConfig.SpawnOffset;

        transform.parent.localPosition = new Vector3(0, 0, -_bpmConverter.GetPositionFromBeat(currentBeat) * editorScale);

        if (_beatmap == null)
        {
            Debug.LogWarning("Beatmap is not loaded. Cannot spawn objects.");
            return;
        }

        var minBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) - spawnOffset);
        var maxBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) + spawnOffset);

        DespawnObjects(minBeat, maxBeat);
        SpawnObjects(editorScale, minBeat, maxBeat);

        BeatLines.SpawnBeatLines(currentBeat, editorScale, spawnOffset, precision);
    }

    private void DespawnObjects(float minBeat, float maxBeat)
    {
        // Despawn objects out of range
        var notesToDespawn = _spawnedNotes
            .Where(note => note.Beat <= minBeat || note.Beat >= maxBeat)
            .ToList();

        var bombsToDespawn = _spawnedBombs
            .Where(bomb => bomb.Beat <= minBeat || bomb.Beat >= maxBeat)
            .ToList();

        var obstaclesToDespawn = _spawnedObstacles
            .Where(obstacle => obstacle.Beat + obstacle.Duration <= minBeat || obstacle.Beat >= maxBeat)
            .ToList();

        foreach (var note in notesToDespawn)
        {
            _mapObjects.DespawnNote(note);
            _spawnedNotes.Remove(note);
        }

        foreach (var bomb in bombsToDespawn)
        {
            _mapObjects.DespawnBomb(bomb);
            _spawnedBombs.Remove(bomb);
        }

        foreach (var obstacle in obstaclesToDespawn)
        {
            _mapObjects.DespawnObstacle(obstacle);
            _spawnedObstacles.Remove(obstacle);
        }
    }

    private void SpawnObjects(float editorScale, float minBeat, float maxBeat)
    {
        // Spawn notes in range that aren't already spawned
        var notesToSpawn = _beatmap.ColorNotes
            .Where(note => note.Beat >= minBeat && note.Beat < maxBeat && !_spawnedNotes.Contains(note));

        var bombsToSpawn = _beatmap.BombNotes
            .Where(bomb => bomb.Beat >= minBeat && bomb.Beat < maxBeat && !_spawnedBombs.Contains(bomb));

        var obstaclesToSpawn = _beatmap.Obstacles
            .Where(obstacle => obstacle.Beat + obstacle.Duration >= minBeat && obstacle.Beat < maxBeat && !_spawnedObstacles.Contains(obstacle));

        foreach (var note in notesToSpawn)
        {
            _mapObjects.SpawnNote(note, editorScale);
            _spawnedNotes.Add(note);
        }

        foreach (var bomb in bombsToSpawn)
        {
            _mapObjects.SpawnBomb(bomb, editorScale);
            _spawnedBombs.Add(bomb);
        }

        foreach (var obstacle in obstaclesToSpawn)
        {
            _mapObjects.SpawnObstacle(obstacle, editorScale);
            _spawnedObstacles.Add(obstacle);
        }

        foreach (Transform child in transform)
        {
            if (child.gameObject.TryGetComponent<ColorNoteObject>(out var noteObject))
            {
                noteObject.SetTransparent();
            }
        }
    }
}