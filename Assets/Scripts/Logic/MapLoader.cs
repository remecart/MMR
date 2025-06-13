using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public sealed class MapLoader : MonoBehaviour
{
    private V3Info _beatmap;
    
    public void LoadMap(string path)
    {
        _beatmap = LoadBeatmap(path);
    }

    private V3Info LoadBeatmap(string path)
    {
        var rawJson = File.ReadAllText(path);
        _beatmap = JsonConvert.DeserializeObject<V3Info>(rawJson);
        
        return _beatmap;
    }
}

[CustomEditor(typeof(MapLoader)), System.Serializable]
public class MapLoaderInterface : Editor
{
    private string _path = @"C:\Users\Remec\BSManager\BSInstances\1.39.1 (1)\Beat Saber_Data\CustomWIPLevels\35204 (SAITAMA 2000 - SpookyBeard)\ExpertPlusStandard.dat"; // temporary hardcode
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        EditorGUILayout.LabelField("Enter Beatmap Path:");
        _path = EditorGUILayout.TextField(_path); // Proper persistent input field
        
        var script = target as MapLoader;

        if (GUILayout.Button("Load Map"))
        {
            script?.LoadMap(_path);
        }
    }
}