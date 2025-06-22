using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class MBotHandler : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;
    
    [FormerlySerializedAs("MapLoader")] public MapLoader mapLoader;

    [AwakeInject] private MapObjects _mapObjects;

    [AwakeInject] private MappingConfig _mappingConfig;

    [FormerlySerializedAs("_mapHandler")] public MapHandler mapHandler;

    [AwakeInject] private readonly BpmConverter _bpmConverter;

    public float intensity;
    public float overshoot;
    public float positionMultiplier;
    public float planeOffset;
    
    
    public ColorNote NextLeft;
    public ColorNote NextRight;
    public ColorNote LastLeft;
    public ColorNote LastRight;

    private bool isPlaying = true;

    public Transform LeftSaber;
    public Transform RightSaber;
    
    public Quaternion targetRotation;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    private void Update()
    {
        PlayBot();
    }

    private void PlayBot()
    {
        if (!isPlaying)
        {
            return;
        }

        if (mapHandler.CurrentBeat == null)
            return;

        var currentBeat = mapHandler.CurrentBeat.Value;
        var editorScale = _mappingConfig.EditorScale;

        GetLastNotes(currentBeat);
        GetNextNotes(currentBeat);

        UpdateMBot(SaberType.Left, LastLeft, NextLeft, currentBeat, editorScale, LeftSaber.gameObject);
        UpdateMBot(SaberType.Right, LastRight, NextRight, currentBeat, editorScale, RightSaber.gameObject);
    }

    private void UpdateMBot(SaberType saberType, ColorNote lastNote, ColorNote nextNote, float currentBeat,
        float editorScale, GameObject saber)
    {
        if (lastNote == null)
        {
            lastNote = new ColorNote();
        }

        if (nextNote == null)
        {
            nextNote = new ColorNote();
        }

        var duration = nextNote.Beat - lastNote.Beat;
        var point = Mathf.Clamp01((currentBeat - lastNote.Beat) / duration);

        var lastAngle = _mapObjects.Rotation(lastNote.Direction) + lastNote.Angle - 90;
        var nextAngle = _mapObjects.Rotation(nextNote.Direction) + nextNote.Angle + 90;
        
        var lastPos = new Vector3((float)lastNote.X - 1.5f, (float)lastNote.Y + 0.5f,
            _bpmConverter.GetPositionFromBeat(lastNote.Beat) * editorScale);
        
        var nextPos = new Vector3((float)nextNote.X - 1.5f, (float)nextNote.Y + 0.5f,
            _bpmConverter.GetPositionFromBeat(nextNote.Beat) * editorScale);
        
        var bezierPos = GetPointOnBezierCurve(lastPos, nextPos, lastAngle, nextAngle, point);
        var CalculatedPos = new Vector3(bezierPos.x + 0.25f, bezierPos.y , planeOffset);

        var direction = (CalculatedPos - saber.transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
        }
        
        saber.transform.rotation = Quaternion.Slerp(saber.transform.rotation, targetRotation, point);

        var xOffset = 0.8f;
        
        if (saberType == SaberType.Left)
        {
            xOffset = -0.8f;
        }
        
        saber.transform.localPosition = new Vector3((CalculatedPos.x - 2) * positionMultiplier / 10 + xOffset,
            CalculatedPos.y * positionMultiplier / 10 + 1f, saber.transform.localPosition.z);

    }

    public Vector2 GetPointOnBezierCurve(Vector2 lastNote, Vector2 nextNote, float lastAngle, float nextAngle, float time)
    {
        float distance = (nextNote - lastNote).magnitude;
        float handleLengthA = distance * 0.3f * intensity + overshoot; // Additive overshoot
        float handleLengthB = distance * 0.3f * intensity + overshoot; // Additive overshoot

        Vector2 controlPointA = lastNote +
                                new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad)) *
                                handleLengthA;
        Vector2 controlPointB = nextNote +
                                new Vector2(Mathf.Cos(nextAngle * Mathf.Deg2Rad), Mathf.Sin(nextAngle * Mathf.Deg2Rad)) *
                                handleLengthB;

        return Mathf.Pow(1 - time, 3) * lastNote +
               3 * Mathf.Pow(1 - time, 2) * time * controlPointA +
               3 * (1 - time) * Mathf.Pow(time, 2) * controlPointB +
               Mathf.Pow(time, 3) * nextNote;
    }

    private void GetLastNotes(float currentBeat)
    {
        LastLeft = mapLoader.Beatmap.ColorNotes
            .Where(note => note.Beat <= currentBeat && note.SaberType == SaberType.Left)
            .OrderByDescending(note => note.Beat)
            .FirstOrDefault() ?? new ColorNote { Beat = currentBeat - 1 };

        LastRight = mapLoader.Beatmap.ColorNotes
            .Where(note => note.Beat <= currentBeat && note.SaberType == SaberType.Right)
            .OrderByDescending(note => note.Beat)
            .FirstOrDefault() ?? new ColorNote { Beat = currentBeat - 1 };
    }

    private void GetNextNotes(float currentBeat)
    {
        NextLeft = mapLoader.Beatmap.ColorNotes
            .Where(note => note.Beat >= currentBeat && note.SaberType == SaberType.Left)
            .OrderBy(note => note.Beat)
            .FirstOrDefault() ?? new ColorNote { Beat = currentBeat + 1 };

        NextRight = mapLoader.Beatmap.ColorNotes
            .Where(note => note.Beat >= currentBeat && note.SaberType == SaberType.Right)
            .OrderBy(note => note.Beat)
            .FirstOrDefault() ?? new ColorNote { Beat = currentBeat + 1 };
    }
}