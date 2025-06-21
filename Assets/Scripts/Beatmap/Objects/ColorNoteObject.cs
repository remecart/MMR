using UnityEngine;

public class ColorNoteObject : MonoBehaviour
{
    [SerializeField] public ColorNote colorNote;
    private Material _material;

    private void Start()
    {
        SetTransparent();
    }

    public void SetTransparent()
    {
        if (this.gameObject.transform.position.z < 0)
        {
            _material.SetInt("_Transparent", 1);
            _material.SetInt("_Unlit", 1);
        }
        else
        {
            _material.SetInt("_Transparent", 0);
            _material.SetInt("_Unlit", 0);
        }
    }

    public void SetNoteColor(Color color)
    {
        _material = gameObject.transform.GetChild(0)
            .GetComponent<MeshRenderer>()
            .material;
        
        if (_material == null)
        {
            Debug.LogError("MeshRenderer material is null for ColorNoteObject.");
            return;
        }

        if (colorNote == null)
        {
            Debug.LogError("ColorNote is not assigned for ColorNoteObject.");
            return;
        }

        _material.color = color;
    }
}