using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ColorNoteObject : MonoBehaviour
{
    [SerializeField] public ColorNote colorNote;

    public void SetNoteColor(Color color)
    {
        var material = gameObject
            .GetComponent<MeshRenderer>()
            .material;

        if (material == null)
        {
            Debug.LogError("MeshRenderer material is null for ColorNoteObject.");
            return;
        }

        if (colorNote == null)
        {
            Debug.LogError("ColorNote is not assigned for ColorNoteObject.");
            return;
        }

        material.color = color;
    }
}