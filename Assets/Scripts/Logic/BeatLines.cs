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
    private List<Matrix4x4> subBeatMatrices = new();

    public int precision;

    public List<GameObject> LineCache;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);

        if (cubeMesh == null)
            cubeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;

        if (cubeMaterial == null)
        {
            cubeMaterial = new Material(Shader.Find("Standard"));
            cubeMaterial.enableInstancing = true;
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

        foreach (Transform child in transform.GetChild(0).transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SpawnBeatLines(float currentBeat, float editorScale, float spawnOffset)
    {
        subBeatMatrices.Clear();

        var minBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) - spawnOffset - 0.25f);
        var maxBeat = _bpmConverter.GetBeatFromRealTime(_bpmConverter.GetRealTimeFromBeat(currentBeat) + spawnOffset + 0.25f);

        minBeat = Math.Clamp(minBeat, 0, 9999);

        for (float beat = Mathf.Ceil(minBeat * precision) / precision; beat <= maxBeat; beat += 1f / precision)
        {
            float tolerance = 0.0125f;
            if (LineCache.Any(go =>
                    float.TryParse(go.name, out var parsed) &&
                    Mathf.Abs(parsed - beat) <= tolerance))
            {
                continue;
            }

            if (Mathf.Abs(Mathf.Round(beat) - beat) < 0.01f)
            {
                var go = Instantiate(BeatLine, transform.GetChild(0).transform, false);
                go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat) * editorScale);
                go.name = $"{beat}";
                go.transform.GetChild(0).gameObject.GetComponent<TextMeshPro>().text = Mathf.Round(beat).ToString(CultureInfo.InvariantCulture);
                LineCache.Add(go);
            }
            else
            {
                var pos = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(beat - currentBeat) * editorScale);
                var scale = new Vector3(4f, 0.01f, 0.025f);
                Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, scale);
                subBeatMatrices.Add(matrix);
            }
        }

        SpawnGuideLines(currentBeat, editorScale, spawnOffset, minBeat, maxBeat);
        DespawnBeatLines(minBeat, maxBeat);
    }

    private List<GameObject> _guideLinePool = new();

    private void SpawnGuideLines(float currentBeat, float editorScale, float spawnOffset, float minBeat, float maxBeat)
    {
        var minZ = _bpmConverter.GetPositionFromBeat(minBeat) * editorScale;
        var maxZ = _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale;
        var centerZ = (minZ + maxZ) / 2f;
        var lengthZ = Mathf.Abs(maxZ - minZ);

        for (var i = 0; i < 5; i++)
        {
            float x = i - 2;

            GameObject line;
            if (i < _guideLinePool.Count && _guideLinePool[i] != null)
            {
                line = _guideLinePool[i];
                line.SetActive(true);
            }
            else
            {
                line = Instantiate(GuideLine, transform.GetChild(1).transform, false);
                if (i >= _guideLinePool.Count)
                    _guideLinePool.Add(line);
                else
                    _guideLinePool[i] = line;
            }

            line.transform.localPosition = new Vector3(x, 0, centerZ);
            line.transform.localScale = new Vector3(0.02f, 0.01f, lengthZ);
        }

        for (var i = 5; i < _guideLinePool.Count; i++)
        {
            if (_guideLinePool[i] != null)
            {
                _guideLinePool[i].SetActive(false);
            }
        }

        HandleEdgeBeatLine(minBeat, maxBeat, editorScale);
    }

    private void HandleEdgeBeatLine(float minBeat, float maxBeat, float editorScale)
    {
        var min = this.transform.GetChild(1).transform.Find("MinBeat");
        var max = this.transform.GetChild(1).transform.Find("MaxBeat");

        if (min == null)
        {
            var go = Instantiate(BeatLine, transform.GetChild(1).transform, false);
            go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(minBeat) * editorScale);
            go.name = $"MinBeat";
            Destroy(go.transform.GetChild(0).gameObject);
        }
        else
        {
            min.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(minBeat) * editorScale);
        }

        if (max == null)
        {
            var go = Instantiate(BeatLine, transform.GetChild(1).transform, false);
            go.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale);
            go.name = $"MaxBeat";
            Destroy(go.transform.GetChild(0).gameObject);
        }
        else
        {
            max.transform.localPosition = new Vector3(0, 0, _bpmConverter.GetPositionFromBeat(maxBeat) * editorScale);
        }
    }

    private void DespawnBeatLines(float minBeat, float maxBeat)
    {
        var linesToDespawn = LineCache
            .Where(line =>
            {
                if (float.TryParse(line.name, out var parsed))
                {
                    return parsed < minBeat || parsed > maxBeat;
                }

                return false;
            })
            .ToList();

        foreach (var line in linesToDespawn)
        {
            LineCache.Remove(line);
            Destroy(line);
        }
    }
}
