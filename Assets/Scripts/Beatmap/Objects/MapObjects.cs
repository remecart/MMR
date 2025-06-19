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
    
    [AwakeInject] private readonly MappingConfig _mappingConfig;
    
    [AwakeInject] private readonly BpmConverter _bpmConverter;

    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _bombPrefab;

    private float _editorScale => _mappingConfig.EditorScale;
    
    public readonly List<GameObject> Notes = new();

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

        // Avoid double-spawning the same note reference
        if (Notes.Any(go =>
            {
                var obj = go.GetComponent<ColorNoteObject>();
                return obj != null && obj.colorNote == note;
            }))
        {
            return;
        }

        var go = Instantiate(_notePrefab, transform, true);
        if (note.X != null && note.Y != null)
        {
            go.transform.localPosition = new Vector3((float)note.X, (float)note.Y,  _bpmConverter.GetPositionFromBeat(note.Beat) * _mappingConfig.EditorScale);
        }

        var colorNoteObject = go.GetComponent<ColorNoteObject>();
        if (colorNoteObject == null)
        {
            Debug.LogError("ColorNoteObject component is missing on the note prefab.");
            Destroy(go);
            return;
        }

        colorNoteObject.colorNote = note;
        colorNoteObject.SetNoteColor(
            note.SaberType == SaberType.Left ? _noteColorConfig.LeftColor : _noteColorConfig.RightColor);

        go.transform.localRotation = Quaternion.Euler(0, 0, Rotation(note.Direction));

        if (note.Direction == 8)
            go.transform.GetChild(2).gameObject.SetActive(false);
        else
            go.transform.GetChild(1).gameObject.SetActive(false);

        Notes.Add(go);
    }

    public void DespawnNote(ColorNote note)
    {
        if (note == null) return;

        var go = Notes
            .FirstOrDefault(n =>
            {
                var obj = n.GetComponent<ColorNoteObject>();
                return obj != null && obj.colorNote == note;
            });

        if (go != null)
        {
            Destroy(go);
            Notes.Remove(go);
        }
    }
    
    public int Rotation(int level)
    {
        return level switch
        {
            0 => 180,
            1 => 0,
            2 => 270,
            3 => 90,
            4 => 225,
            5 => 135,
            6 => 315,
            7 => 45,
            _ => 0
        };
    }
}
