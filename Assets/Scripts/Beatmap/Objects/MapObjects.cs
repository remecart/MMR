using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapObjects : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;
    [AwakeInject] private readonly NoteColorConfig _noteColorConfig;

    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _bombPrefab;

    private readonly Dictionary<float, GameObject> _notes = new();

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    public void SpawnNote(ColorNote note)
    {
        if (_notePrefab == null)
        {
            Debug.Log("Note prefab is not assigned in MapObjects.");
            return;
        }

        if (note == null)
        {
            Debug.Log("ColorNote is null in MapObjects.");
            return;
        }

        if (_notes.ContainsKey(note.Beat))
        {
            return; // Schon gespawnt
        }

        var go = Instantiate(_notePrefab, transform, true);
        if (note.X != null && note.Y != null)
        {
            go.transform.localPosition = new Vector3((float)note.X, (float)note.Y, note.Beat);
        }

        var colorNoteObject = go.GetComponent<ColorNoteObject>();
        colorNoteObject.colorNote = note;

        if (colorNoteObject == null)
        {
            Debug.LogError("ColorNoteObject component is missing on the note prefab.");
            Destroy(go);
            return;
        }

        colorNoteObject.SetNoteColor(note.SaberType == SaberType.Left
            ? _noteColorConfig.LeftColor
            : _noteColorConfig.RightColor);

        var direction = GetEulerAnglesFromDirection(note.Direction);
        go.transform.localRotation = Quaternion.Euler(direction);
        go.transform.rotation = Quaternion.Euler(-90, 0, 0);

        if (note.Direction == 8)
        {
            go.transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            go.transform.GetChild(1).gameObject.SetActive(false);
        }

        _notes[note.Beat] = go;
    }

    public void DespawnNote(ColorNote note)
    {
        if (note == null)
        {
            return;
        }

        if (!_notes.TryGetValue(note.Beat, out var go))
        {
            return;
        }

        if (go != null)
        {
            Destroy(go);
        }

        _notes.Remove(note.Beat);
    }

    private static Vector3 GetEulerAnglesFromDirection(int direction)
    {
        return direction switch
        {
            0 => new Vector3(0, 0, 0) // Up
            ,
            1 => new Vector3(0, 0, 180) // Down
            ,
            2 => new Vector3(0, 0, 90) // Left
            ,
            3 => new Vector3(0, 0, 270) // Right
            ,
            4 => new Vector3(0, 0, 45) // Up-Left
            ,
            5 => new Vector3(0, 0, 315) // Up-Right
            ,
            6 => new Vector3(0, 0, 135) // Down-Left
            ,
            7 => new Vector3(0, 0, 225) // Down-Right
            ,
            8 => new Vector3(0, 0, 0) // Any direction (or random if needed)
            ,
            _ => Vector3.zero
        };
    }
}