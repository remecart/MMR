using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleFrame : MonoBehaviour
{
    public Mesh edgeMesh;
    public Material edgeMaterial;
    public float edgeThickness = 0.05f;

    private Matrix4x4[] matrices;

    private static readonly Vector3[] edgePairs = new Vector3[]
    {
        // 12 edges of a unit cube
        new Vector3(-0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f, -0.5f),
        new Vector3( 0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
        new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-0.5f, -0.5f, -0.5f),

        new Vector3(-0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f,  0.5f),
        new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f, -0.5f),

        new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f, -0.5f, -0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3( 0.5f, -0.5f,  0.5f), new Vector3( 0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
    };

    void Start()
    {
        matrices = new Matrix4x4[edgePairs.Length / 2];
    }

    void LateUpdate()
    {
        for (int i = 0; i < edgePairs.Length; i += 2)
        {
            Vector3 worldStart = transform.TransformPoint(edgePairs[i]);
            Vector3 worldEnd = transform.TransformPoint(edgePairs[i + 1]);

            Vector3 dir = worldEnd - worldStart;
            Vector3 mid = (worldStart + worldEnd) * 0.5f;
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            Vector3 scale = new Vector3(edgeThickness, edgeThickness, dir.magnitude);

            matrices[i / 2] = Matrix4x4.TRS(mid, rot, scale);
        }

        Graphics.DrawMeshInstanced(edgeMesh, 0, edgeMaterial, matrices);
    }
}
