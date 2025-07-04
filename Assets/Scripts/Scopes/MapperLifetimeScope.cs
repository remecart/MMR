using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class MapperLifetimeScope : LifetimeScope
{
    [SerializeField]
    private MapLoader _mapLoader;

    [SerializeField]
    private MapPlacementHandler _mapPlacementHandler;

    [SerializeField]
    private MapObjects _mapObjects;

    [FormerlySerializedAs("mapInfoLoader")]
    [FormerlySerializedAs("_readMapInfo")]
    [SerializeField]
    private MapInfoLoader _mapInfoLoader;

    [SerializeField]
    private BpmConverter _bpmConverter;

    [SerializeField]
    private SongLoader _songLoader;

    [SerializeField]
    private MapHandler _mapHandler;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_mapInfoLoader);
        builder.RegisterInstance(_mapLoader);
        builder.RegisterInstance(_mapHandler);
        builder.RegisterInstance(_mapObjects);
        builder.RegisterInstance(_bpmConverter);
        builder.RegisterInstance(_songLoader);
        builder.RegisterInstance(_mapPlacementHandler);
    }
}