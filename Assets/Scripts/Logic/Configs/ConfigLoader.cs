using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

public class ConfigLoader
{
    private readonly static JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        Converters =
        {
            new ColorConverter(),
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new QuaternionConverter(),
        },
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly string _configPath;
    private readonly Dictionary<Type, IConfig> _loadedConfigs = new Dictionary<Type, IConfig>();

    public ConfigLoader()
    {
        JsonConvert.DefaultSettings = () => SerializerSettings;

        _configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Micro-Mapper-Rewrite", "Configs");

        if (!Directory.Exists(_configPath))
        {
            Directory.CreateDirectory(_configPath);
        }

        LoadAllConfigs();
    }

    public T Get<T>() where T : class, IConfig
    {
        return _loadedConfigs.TryGetValue(typeof(T), out var config) ? config as T : null;
    }

    public IEnumerable<IConfig> GetAll() => _loadedConfigs.Values;

    private void LoadAllConfigs()
    {
        var configTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IConfig).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (var type in configTypes)
        {
            var attr = type.GetCustomAttribute<ConfigFileNameAttribute>();
            if (attr == null)
            {
                continue;
            }

            var filePath = Path.Combine(_configPath, attr.FileName);

            try
            {
                IConfig config;
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    config = (IConfig)JsonConvert.DeserializeObject(json, type);
                }
                else // Create new config if it doesn't exist
                {
                    config = (IConfig)Activator.CreateInstance(type);
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(config, Formatting.Indented));
                }

                if (config != null)
                {
                    _loadedConfigs[type] = config;
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to load config {type.Name}: {e.Message}");
            }
        }
    }
}