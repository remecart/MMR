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
    [SerializeField] private GameObject _obstaclePrefab;

    private float _editorScale => _mappingConfig.EditorScale;

    public readonly List<GameObject> Notes = new();
    public readonly List<GameObject> Bombs = new();
    public readonly List<GameObject> Obstacles = new();

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    public void SpawnNote(ColorNote note, float editorScale)
    {
        Debug.Log("Spawning note: " + note);
        
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
            go.transform.localPosition = new Vector3((float)note.X - 1.5f, (float)note.Y + 0.5f,
                _bpmConverter.GetPositionFromBeat(note.Beat) * editorScale);
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

    public void SpawnBomb(BombNote bomb, float editorScale)
    {
        if (_bombPrefab == null)
        {
            Debug.Log("Bomb prefab is not assigned in MapObjects.");
            return;
        }

        if (bomb == null)
        {
            Debug.Log("BombNote is null in MapObjects.");
            return;
        }

        // Avoid double-spawning the same bomb reference
        if (Bombs.Any(go =>
            {
                if (go == null) return false; // Unity null check
                var obj = go.GetComponent<BombNoteObject>();
                return obj != null && obj.bombNote == bomb;
            }))
        {
            return;
        }

        var go = Instantiate(_bombPrefab, transform, true);
        if (bomb.X != null && bomb.Y != null)
        {
            go.transform.localPosition = new Vector3((float)bomb.X - 1.5f, (float)bomb.Y + 0.5f,
                _bpmConverter.GetPositionFromBeat(bomb.Beat) * editorScale);
        }

        var bombNoteObject = go.GetComponent<BombNoteObject>();

        bombNoteObject.bombNote = bomb;
        Bombs.Add(go);
    }

    public void SpawnObstacle(Obstacle obstacle, float editorScale)
    {
        if (_obstaclePrefab == null)
        {
            Debug.Log("Obstacle prefab is not assigned in MapObjects.");
            return;
        }

        if (obstacle == null)
        {
            Debug.Log("Obstacle is null in MapObjects.");
            return;
        }

        // Avoid double-spawning the same bomb reference
        if (Obstacles.Any(go =>
            {
                if (go == null) return false; // Unity null check
                var obj = go.GetComponent<ObstacleObject>();
                return obj != null && obj.obstacle == obstacle;
            }))
        {
            return;
        }

        var go = Instantiate(_obstaclePrefab, transform, true);
        if (obstacle.X != null && obstacle.Y != null)
        {
            var zPos = _bpmConverter.GetPositionFromBeat(obstacle.Beat + obstacle.Duration) -
                       _bpmConverter.GetPositionFromBeat(obstacle.Beat);
            go.transform.localPosition = new Vector3((float)obstacle.X - 1.5f,
                Mathf.Clamp(Mathf.Clamp((float)obstacle.Y, 0f, 2f) + (float)obstacle.Height / 2, 0f, 5.5f) - 0.5f,
                (_bpmConverter.GetPositionFromBeat(obstacle.Beat) + (zPos / 2)) * editorScale);
            go.transform.localScale = new Vector3(obstacle.Width, obstacle.Height, zPos * editorScale);
            
            // smr.SetBlendShapeWeight(0, 800 * go.transform.localScale.x);
            // smr.SetBlendShapeWeight(1, 800 * go.transform.localScale.y);
            // smr.SetBlendShapeWeight(2, 800 * go.transform.localScale.z);
        }

        var obstacleObject = go.GetComponent<ObstacleObject>();

        obstacleObject.obstacle = obstacle;
        Obstacles.Add(go);
    }

    public void DespawnObstacle(Obstacle obstacle)
    {
        if (obstacle == null) return;

        var go = Obstacles
            .FirstOrDefault(n =>
            {
                if (n == null) return false; // Unity null check
                var obj = n.GetComponent<ObstacleObject>();
                return obj != null && obj.obstacle == obstacle;
            });

        if (go != null)
        {
            Destroy(go);
            Obstacles.Remove(go);
        }
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

    public void DespawnBomb(BombNote bomb)
    {
        if (bomb == null) return;

        var go = Bombs
            .FirstOrDefault(n =>
            {
                if (n == null) return false; // Unity null check
                var obj = n.GetComponent<BombNoteObject>();
                return obj != null && obj.bombNote == bomb;
            });

        if (go != null)
        {
            Destroy(go);
            Bombs.Remove(go);
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