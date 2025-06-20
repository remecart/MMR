using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BpmConverter : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;
    
    [AwakeInject] private MapLoader _mapLoader;
    
    [AwakeInject] private ReadMapInfo _readMapInfo;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }
    
    public float GetPositionFromBeat(float beat)
    {
        var bpmChanges = _mapLoader.Beatmap.BpmEvents;
        var position = 0f;
        var previousBpm = _readMapInfo.info._beatsPerMinute;
        var previousBeat = 0f;

        foreach (var bpmEvent in bpmChanges)
        {
            if (beat <= bpmEvent.Beat)
            {
                break;
            }

            var duration = bpmEvent.Beat - previousBeat;
            position += duration * (60f / previousBpm);

            previousBpm = bpmEvent.Multiplier;
            previousBeat = bpmEvent.Beat;
        }

        var remainingDuration = beat - previousBeat;
        position += remainingDuration * (60f / previousBpm);

        return position;
    }
    
    public float GetBeatFromPosition(float position)
    {
        var bpmChanges = _mapLoader.Beatmap.BpmEvents;
        float beat = 0;
        float accumulatedPosition = 0;
        for (var i = 0; i < bpmChanges.Count; i++)
        {
            if (i + 1 < bpmChanges.Count)
            {
                var nextBeatTime = bpmChanges[i + 1].Beat;
                var duration = nextBeatTime - bpmChanges[i].Beat;
                var sectionPosition = duration * (60f / bpmChanges[i].Multiplier);

                if (accumulatedPosition + sectionPosition >= position)
                {
                    var remainingPosition = position - accumulatedPosition;
                    beat = bpmChanges[i].Beat + (remainingPosition / (60f / bpmChanges[i].Multiplier));
                    return beat;
                }
                else
                {
                    accumulatedPosition += sectionPosition;
                }
            }
            else
            {
                var remainingPosition = position - accumulatedPosition;
                beat = bpmChanges[i].Beat + (remainingPosition / (60f / bpmChanges[i].Multiplier));
                return beat;
            }
        }

        return beat;
    }

    public float GetBpmAtBeat(float beat)
    {
        var bpmChanges = _mapLoader.Beatmap.BpmEvents;
        
        var previousBpmChange = bpmChanges
            .Where(e => e.Beat <= beat)
            .OrderByDescending<BpmEvent, object>(e => e.Beat)
            .FirstOrDefault();

        if (previousBpmChange != null)
        {
            return previousBpmChange.Multiplier;
        }

        return _readMapInfo.info._beatsPerMinute;
    }
    
    public float GetBeatFromRealTime(float realTime)
    {
        var bpmEvents = _mapLoader.Beatmap.BpmEvents;
        var accumulatedTime = 0.0;

        for (var i = 0; i < bpmEvents.Count; i++)
        {
            if (i + 1 < bpmEvents.Count)
            {
                // Calculate the duration until the next BPM change (use double for accuracy)
                var nextBeatTime = bpmEvents[i + 1].Beat;
                var bpmDuration = (nextBeatTime - bpmEvents[i].Beat) * (60.0 / bpmEvents[i].Multiplier);

                if (accumulatedTime + bpmDuration >= realTime)
                {
                    var remainingTime = realTime - accumulatedTime;
                    return (float)(bpmEvents[i].Beat + (remainingTime / (60.0 / bpmEvents[i].Multiplier)));
                }

                accumulatedTime += bpmDuration;
            }
            else
            {
                // Calculate for the last BPM event
                var remainingTime = realTime - accumulatedTime;
                return (float)(bpmEvents[i].Beat + (remainingTime / (60.0 / bpmEvents[i].Multiplier)));
            }
        }

        return bpmEvents.Count > 0 ? (float)bpmEvents[bpmEvents.Count - 1].Beat : 0f;
    }
    
    public float GetRealTimeFromBeat(float beat)
    {
        var bpmEvents = _mapLoader.Beatmap.BpmEvents;
        var time = 0f;

        var currentBpm = _readMapInfo.info._beatsPerMinute;
        var lastBeat = 0f;

        foreach (var bpmEvent in bpmEvents)
        {
            if (beat <= bpmEvent.Beat)
            {
                time += (beat - lastBeat) * (60f / currentBpm);
                return time;
            }

            time += (bpmEvent.Beat - lastBeat) * (60f / currentBpm);
            currentBpm = bpmEvent.Multiplier;
            lastBeat = bpmEvent.Beat;
        }

        time += (beat - lastBeat) * (60f / currentBpm);
        return time;
    }

}
