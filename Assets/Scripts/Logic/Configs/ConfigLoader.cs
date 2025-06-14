using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

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
            new KeyCodeConverter()
        },
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    private readonly string _configPath;
    private readonly Dictionary<Type, IConfig> _loadedConfigs = new Dictionary<Type, IConfig>();
    private readonly Dictionary<Type, string> _originalConfigs = new Dictionary<Type, string>();


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

    public bool IsChanged<T>() where T : class, IConfig
    {
        var type = typeof(T);
        if (!_loadedConfigs.TryGetValue(type, out var config) || !_originalConfigs.TryGetValue(type, out var originalJson))
        {
            return false;
        }

        var currentJson = JsonConvert.SerializeObject(config, Formatting.Indented);
        return !string.Equals(currentJson, originalJson, StringComparison.Ordinal);
    }

    public bool IsChanged(Type type)
    {
        if (!typeof(IConfig).IsAssignableFrom(type))
        {
            return false;
        }

        if (!_loadedConfigs.TryGetValue(type, out var config) || !_originalConfigs.TryGetValue(type, out var originalJson))
        {
            return false;
        }

        var currentJson = JsonConvert.SerializeObject(config, Formatting.Indented);
        return !string.Equals(currentJson, originalJson, StringComparison.Ordinal);
    }


    public void SaveChanges<T>() where T : class, IConfig
    {
        var type = typeof(T);
        if (!_loadedConfigs.TryGetValue(type, out var config))
        {
            return;
        }

        var attr = type.GetCustomAttribute<ConfigFileNameAttribute>();
        if (attr == null)
        {
            return;
        }

        var filePath = Path.Combine(_configPath, attr.FileName);

        var json = JsonConvert.SerializeObject(config, Formatting.Indented);

        File.WriteAllText(filePath, json);
        _originalConfigs[type] = json;
    }

    public void SaveChanges(Type type)
    {
        if (!typeof(IConfig).IsAssignableFrom(type))
        {
            throw new ArgumentException("Type must implement IConfig", nameof(type));
        }

        if (!_loadedConfigs.TryGetValue(type, out var config))
        {
            return;
        }

        var attr = type.GetCustomAttribute<ConfigFileNameAttribute>();
        if (attr == null)
        {
            return;
        }

        var filePath = Path.Combine(_configPath, attr.FileName);

        var json = JsonConvert.SerializeObject(config, Formatting.Indented);

        File.WriteAllText(filePath, json);
        _originalConfigs[type] = json;
    }

    public void RevertChanges<T>() where T : class, IConfig
    {
        var type = typeof(T);
        if (!_originalConfigs.TryGetValue(type, out var originalJson))
        {
            return;
        }

        if (JsonConvert.DeserializeObject(originalJson, type) is IConfig config)
        {
            _loadedConfigs[type] = config;
        }
    }

    public void RevertChanges(Type type)
    {
        if (!typeof(IConfig).IsAssignableFrom(type))
        {
            throw new ArgumentException("Type must implement IConfig", nameof(type));
        }

        if (!_originalConfigs.TryGetValue(type, out var originalJson))
        {
            return;
        }

        if (JsonConvert.DeserializeObject(originalJson, type) is IConfig config)
        {
            _loadedConfigs[type] = config;
        }
    }

    private void LoadAllConfigs()
    {
        var configTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IConfig).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

        foreach (var type in configTypes)
        {
            var attr = type.GetCustomAttribute<ConfigFileNameAttribute>();
            if (attr == null)
                continue;

            var filePath = Path.Combine(_configPath, attr.FileName);

            try
            {
                IConfig config;
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    config = JsonConvert.DeserializeObject(json, type) as IConfig;
                    if (config != null)
                    {
                        _loadedConfigs[type] = config;
                        _originalConfigs[type] = JsonConvert.SerializeObject(config);
                    }
                }
                else
                {
                    config = (IConfig)Activator.CreateInstance(type);
                    var json = JsonConvert.SerializeObject(config);
                    File.WriteAllText(filePath, json);
                    _loadedConfigs[type] = config;
                    _originalConfigs[type] = json;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load config {type.Name}: {e.Message}");
            }
        }
    }
}