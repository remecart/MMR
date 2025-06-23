using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public sealed class MapLoader : MonoBehaviour
{
    [Inject]
    private readonly LifetimeScope _scope;

    [CanBeNull]
    private V3Info _beatmap;

    public Action<V3Info> OnMapLoaded;
    public V3Info Beatmap => _beatmap ?? throw new MapNotLoadedException();

    [AwakeInject]
    private readonly MapInfoLoader _mapInfoLoader;

    private void Awake()
    {
        AwakeInjector.InjectInto(this, _scope);
    }

    public void Start()
    {
        var path = $"{_mapInfoLoader.folderPath}\\ExpertPlusStandard.dat";
        LoadMap(path);
    }

    public void LoadMap(string path)
    {
        _beatmap = LoadBeatmap(path);

        if (_beatmap == null)
        {
            Debug.LogError("Failed to load beatmap from path: " + path);
            return;
        }

        SortBeatmapObjectLists(_beatmap);
        OnMapLoaded?.Invoke(_beatmap);
    }

    private void SortBeatmapObjectLists(object beatmap)
    {
        var beatmapType = beatmap.GetType();
        var properties = beatmapType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite)
            {
                continue;
            }

            var propType = prop.PropertyType;
            if (!propType.IsGenericType || propType.GetGenericTypeDefinition() != typeof(List<>))
            {
                continue;
            }

            var elementType = propType.GetGenericArguments()[0];
            if (!typeof(BeatmapObject).IsAssignableFrom(elementType))
            {
                continue;
            }

            if (prop.GetValue(beatmap) is not IList list || list.Count == 0)
            {
                continue;
            }

            var beatProperty = elementType.GetProperty("Beat", BindingFlags.Public | BindingFlags.Instance);
            if (beatProperty == null || beatProperty.PropertyType != typeof(float))
            {
                continue;
            }

            var comparison = CreateBeatComparison(elementType, beatProperty);
            var sortMethod = propType.GetMethod("Sort", new[]
            {
                comparison.GetType()
            });
            if (sortMethod == null)
            {
                continue;
            }

            sortMethod.Invoke(list, new object[]
            {
                comparison
            });
        }
    }

    private static Delegate CreateBeatComparison(Type elementType, PropertyInfo beatProperty)
    {
        var aParam = Expression.Parameter(elementType, "a");
        var bParam = Expression.Parameter(elementType, "b");

        var aBeat = Expression.Property(aParam, beatProperty);
        var bBeat = Expression.Property(bParam, beatProperty);

        var compareCall = Expression.Call(
                                          aBeat,
                                          typeof(float).GetMethod("CompareTo", new[]
                                          {
                                              typeof(float)
                                          }) ?? throw new InvalidOperationException("CompareTo method not found."),
                                          bBeat
                                         );

        var lambda = Expression.Lambda(compareCall, aParam, bParam);

        return Delegate.CreateDelegate(typeof(Comparison<>)
                                           .MakeGenericType(elementType), lambda.Compile(), "Invoke");
    }

    private V3Info LoadBeatmap(string path)
    {
        var rawJson = File.ReadAllText(path);
        _beatmap = JsonConvert.DeserializeObject<V3Info>(rawJson);

        return _beatmap;
    }
}