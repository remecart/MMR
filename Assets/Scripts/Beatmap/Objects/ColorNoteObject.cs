using System;
using UnityEngine;
using VContainer;

public class ColorNoteObject : MonoBehaviour
{
    [Inject]
    private readonly NoteColorConfig _noteColorConfig;
    
    [SerializeField]
    public ColorNote colorNote;

    private void Start()
    {
        var material = gameObject.GetComponent<MeshRenderer>().material;

        if (colorNote.SaberType == SaberType.Left)
        {
            material.color = _noteColorConfig.LeftColor;
        }
        else material.color = _noteColorConfig.RightColor;
    }
}
