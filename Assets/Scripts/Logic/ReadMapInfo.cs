using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Application = UnityEngine.Application;

public class ReadMapInfo : MonoBehaviour
{

    [Header("Map Info")]
    public Info info;

    [Header("Settings")]
    public string folderPath;

    // Start is called before the first frame update
    void Start()
    {
        string infoPath = "info.dat";

        if (!folderPath.Contains("info.dat"))
        {
            infoPath = "Info.dat";
        }

        if (!Application.isEditor)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1 && arguments[1].Trim() != "")
            {
                folderPath = Path.GetDirectoryName(arguments[1]) + "\\";
            }

            string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
            info = JsonUtility.FromJson<Info>(rawData);

            //if (arguments.Length > 1)
            //{
            //    if (!string.IsNullOrEmpty(arguments[1]))
            //    {
            //        
            //    }
            //}
        }
        else
        {
            string rawData = File.ReadAllText(Path.Combine(folderPath, infoPath));
            info = JsonUtility.FromJson<Info>(rawData);
        }
    }

    void OnApplicationQuit()
    {
        if (File.Exists(Path.Combine(folderPath, "imgui.ini")))
        {
            File.Delete(Path.Combine(folderPath, "imgui.ini"));
        }
    }
}

[System.Serializable]
public class Info
{
    public string _version = "2.1.0";
    public string _songName = "";
    public string _songSubName = "";
    public string _songAuthorName = "";
    public string _levelAuthorName = "";
    public float _beatsPerMinute = 100;
    public float _shuffle = 0;
    public float _shufflePeriod = 0;
    public float _previewStartTime = 0;
    public float _previewDuration = 10;
    public string _songFilename = "";
    public string _coverImageFilename = "";
    public string _environmentName = "DefaultEnvironment";
    public float _songTimeOffset = 0;
    public _customData _customData = new _customData();
    public List<_difficultyBeatmapSets> _difficultyBeatmapSets = new List<_difficultyBeatmapSets>();
}

[System.Serializable]
public class _customData
{
    public _editors _editors = new _editors();
}

[System.Serializable]
public class _editors
{
    public string _lastEditedBy = "Micro Mapper";
}

[System.Serializable]
public class _difficultyBeatmapSets
{
    public string _beatmapCharacteristicName = "Standard";
    public List<_difficultyBeatmaps> _difficultyBeatmaps;
}

[System.Serializable]
public class _difficultyBeatmaps
{
    public string _difficulty = "ExpertPlus";
    public int _difficultyRank = 7;
    public string _beatmapFilename = "ExpertPlusStandard.dat";
    public float _noteJumpMovementSpeed = 20;
    public float _noteJumpStartBeatOffset = 0;
    public int _beatmapColorSchemeIdx = 0;
    public int _environmentNameIdx = 0;
    public _difficultyBeatmapsCustomData _customData;
}

[System.Serializable]
public class _difficultyBeatmapsCustomData
{
    public List<string> _requirements = new List<string>();
    public string _difficultyLabel = "";
}