using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BeatLines : MonoBehaviour
{
    [Inject] private readonly LifetimeScope _scope;
    [AwakeInject] private BpmConverter _bpmConverter;

    public GameObject BeatLine;
    public GameObject SubBeatLine;
    public GameObject GuideLine;

    public Mesh cubeMesh;
    public Material cubeMaterial;
    
    public List<GameObject> LineCache;

    public List<Matrix4x4> subBeatMatrices = new();
    private readonly Queue<GameObject> beatLinePool = new();
    private readonly List<GameObject> guideLinePool = new();

    private float _lastBeat = -1f;
    private float _lastOffset = -1f;

    private const int MaxSubBeatCount = 1000;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);

        if (cubeMesh == null)
            cubeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;

        if (cubeMaterial == null)
        {
            cubeMaterial = new Material(Shader.Find("Standard")) { enableInstancing = true };
        }
    }

    private void Start()
    {
        subBeatMatrices.Clear();
    }

    private void Update()
    {
        if (subBeatMatrices.Count > 0)
        {
            Graphics.DrawMeshInstanced(
                cubeMesh,
                0,
                cubeMaterial,
                subBeatMatrices
            );
        }
    }

    public void ClearLines()
    {
        LineCache.Clear();

        foreach (Transform child in transform.GetChild(0))
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in transform.GetChild(1))
        {
            Destroy(child.gameObject);
        }

        beatLinePool.Clear();
        guideLinePool.Clear();
    }

    public void ResetLines()
    {
        ClearLines();
        subBeatMatrices.Clear();
        _lastBeat = -1f;
        _lastOffset = -1f;
    }
    
    public void SpawnBeatLines(float currentBeat, float editorScale, float spawnOffset, int precision)
    {
        if (Mathf.Approximately(currentBeat, _lastBeat) && Mathf.Approximately(spawnOffset, _lastOffset)) return;

        _lastBeat = currentBeat;
        _lastOffset = spawnOffset;
        subBeatMatrices.Clear();

        float adaptivePrecision = Mathf.Max(1, Mathf.FloorToInt(precision / spawnOffset));
        float minBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) - spawnOffset - 0.25f);
        float maxBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) + spawnOffset + 0.25f);

        minBeat = Mathf.Clamp(minBeat, 0, 9999);

        for (float beat = Mathf.Ceil(minBeat * precision) / precision; beat <= maxBeat; beat += 1f / precision)
        {
            if (Mathf.Abs(Mathf.Round(beat) - beat) < 0.01f)
            {
                if (!LineCache.Any(go => Mathf.Abs(float.Parse(go.name) - beat) < 0.0125f))
                {
                    GameObject go = GetOrCreateBeatLine();
                    go.transform.SetParent(transform.GetChild(0), false);
                    go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat) * editorScale);
                    go.name = $"{beat}";
                    go.GetComponentInChildren<TextMeshPro>().text = Mathf.RoundToInt(beat).ToString();
                    LineCache.Add(go);
                }
            }
            else if (subBeatMatrices.Count < MaxSubBeatCount)
            {
                Vector3 pos = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat) * editorScale - _bpmConverter.GetPositionFromBeat(currentBeat) * editorScale);
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, new Vector3(4f, 0.01f, 0.025f));
                subBeatMatrices.Add(matrix);
            }
        }

        SpawnGuideLines(currentBeat, editorScale, spawnOffset, minBeat, maxBeat);
        DespawnBeatLines(minBeat, maxBeat);
    }

    private GameObject GetOrCreateBeatLine()
    {
        if (beatLinePool.Count > 0)
        {
            GameObject pooled = beatLinePool.Dequeue();
            pooled.SetActive(true);
            return pooled;
        }

        return Instantiate(BeatLine);
    }

    private void DespawnBeatLines(float minBeat, float maxBeat)
    {
        var toDespawn = LineCache
            .Where(line => float.TryParse(line.name, out var parsed) && (parsed < minBeat || parsed > maxBeat))
            .ToList();

        foreach (var line in toDespawn)
        {
            LineCache.Remove(line);
            line.SetActive(false);
            beatLinePool.Enqueue(line);
        }
    }

    private void SpawnGuideLines(float currentBeat, float editorScale, float spawnOffset, float minBeat, float maxBeat)
    {
        float minZ = _bpmConverter.GetPositionFromBeat(minBeat) * editorScale;
        float maxZ = _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale;
        float centerZ = (minZ + maxZ) / 2f;
        float lengthZ = Mathf.Abs(maxZ - minZ);

        for (int i = 0; i < 5; i++)
        {
            float x = i - 2;

            GameObject guide;
            if (i < guideLinePool.Count && guideLinePool[i] != null)
            {
                guide = guideLinePool[i];
                guide.SetActive(true);
            }
            else
            {
                guide = Instantiate(GuideLine, transform.GetChild(1), false);
                if (i >= guideLinePool.Count)
                    guideLinePool.Add(guide);
                else
                    guideLinePool[i] = guide;
            }

            guide.transform.localPosition = new Vector3(x, 0, centerZ);
            guide.transform.localScale = new Vector3(0.02f, 0.01f, lengthZ);
        }

        for (int i = 5; i < guideLinePool.Count; i++)
        {
            if (guideLinePool[i] != null)
                guideLinePool[i].SetActive(false);
        }

        HandleEdgeBeatLine(minBeat, maxBeat, editorScale);
    }

    private void HandleEdgeBeatLine(float minBeat, float maxBeat, float editorScale)
    {
        var parent = transform.GetChild(1);

        Transform min = parent.Find("MinBeat");
        if (min == null)
        {
            GameObject go = Instantiate(BeatLine, parent, false);
            go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(minBeat) * editorScale);
            go.name = "MinBeat";
            Destroy(go.transform.GetChild(0).gameObject);
        }
        else
        {
            min.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(minBeat) * editorScale);
        }

        Transform max = parent.Find("MaxBeat");
        if (max == null)
        {
            GameObject go = Instantiate(BeatLine, parent, false);
            go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale);
            go.name = "MaxBeat";
            Destroy(go.transform.GetChild(0).gameObject);
        }
        else
        {
            max.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale);
        }
    }
}