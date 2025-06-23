using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Application = UnityEngine.Application;

public class MapInfoLoader : MonoBehaviour
{
    public Info Info;

    public string folderPath;

    // Start is called before the first frame update
    private void Start()
    {
        var files = Directory.GetFiles(folderPath, "info.dat", SearchOption.AllDirectories);

        var infoPath = files
                           .FirstOrDefault(x => x
                                               .EndsWith("info.dat", StringComparison.OrdinalIgnoreCase))
                       ?? throw new FileNotFoundException("info.dat file not found in the specified directory.");

        Debug.Log(infoPath);

        if (!Application.isEditor)
        {
            var arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1 && !string.IsNullOrWhiteSpace(arguments[1]))
            {
                folderPath = new StringBuilder()
                             .Append(Path.GetDirectoryName(arguments[1]))
                             .Append(Path.DirectorySeparatorChar)
                             .ToString();
            }
        }

        try
        {
            var rawData = File.ReadAllText(infoPath);
            Debug.Log(rawData);
            Info = JsonConvert.DeserializeObject<Info>(rawData)
                   ?? throw new InvalidOperationException("Failed to deserialize Info data");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading map info: {ex.Message}");
        }
    }

    private void OnApplicationQuit()
    {
        if (File.Exists(Path.Combine(folderPath, "imgui.ini")))
        {
            File.Delete(Path.Combine(folderPath, "imgui.ini"));
        }
    }
}