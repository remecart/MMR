using System;
using Newtonsoft.Json;

[JsonObject]
public interface IConfig
{
}

/// <summary>
/// Used to identify the config file name.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigFileNameAttribute : Attribute
{
    public string FileName { get; }

    public ConfigFileNameAttribute(string fileName)
    {
        FileName = fileName;
    }
}