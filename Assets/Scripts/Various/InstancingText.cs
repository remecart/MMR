using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstancingTest : MonoBehaviour
{
    public Material cubeMaterial;
    private Mesh cubeMesh;
    private List<Matrix4x4> matrices = new();

    void Start()
    {
        cubeMesh = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>().sharedMesh;
        var mat = new Material(Shader.Find("Standard"));
        mat.enableInstancing = true;
        cubeMaterial = mat;

        matrices.Add(Matrix4x4.TRS(new Vector3(0, 0, 5), Quaternion.identity, Vector3.one));
    }

    void Update()
    {
        Graphics.DrawMeshInstanced(cubeMesh, 0, cubeMaterial, matrices);
    }
}
