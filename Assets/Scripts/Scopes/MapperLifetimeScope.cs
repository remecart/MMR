using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapperLifetimeScope : LifetimeScope
{
    [SerializeField]
    private MapLoader _mapLoader;
    
    [SerializeField]
    private MapObjects _mapObjects;
    
    [SerializeField]
    private ReadMapInfo _readMapInfo;
    
    [SerializeField]
    private BpmConverter _bpmConverter;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_mapLoader);
        builder.RegisterInstance(_mapObjects);
        builder.RegisterInstance(_readMapInfo);
        builder.RegisterInstance(_bpmConverter);
    }
}