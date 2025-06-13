using UnityEngine;
using VContainer;

public class NoteColorer : MonoBehaviour
{
    [Inject]
    private readonly NoteColorConfig _noteColorConfig;

    private void Start()
    {
        if (_noteColorConfig == null)
        {
            Debug.LogError("NoteColorConfig is not injected properly.");
            return;
        }

        Debug.Log("Note Color Config Loaded: " + _noteColorConfig.ToString());
    }
}