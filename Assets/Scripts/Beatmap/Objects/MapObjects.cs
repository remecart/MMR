using System.Collections.Generic;
using UnityEngine;

public class MapObjects : MonoBehaviour
{
    [SerializeField]
    private GameObject _notePrefab;
    [SerializeField]
    private GameObject _bombPrefab;

    public void SpawnNote(ColorNote note)
    {
        var go = Instantiate(_notePrefab, transform, true);
        go.transform.localPosition = new Vector3((float)note.X, (float)note.Y, note.Beat);
        
    }
}
