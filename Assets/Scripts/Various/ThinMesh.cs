using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ThinMesh : MonoBehaviour
{
    [Range(0.01f, 1f)]
    public float thickness = 0.2f;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh mesh = GetComponent<MeshFilter>().mesh; // instead of sharedMesh
        Vector3[] vertices = mesh.vertices;

        foreach (Vector3 v in mesh.vertices)
            Debug.DrawRay(transform.TransformPoint(v), Vector3.up * 0.1f, Color.red, 5f);

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }
}