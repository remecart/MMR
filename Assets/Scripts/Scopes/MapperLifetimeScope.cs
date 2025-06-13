using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapperLifetimeScope : LifetimeScope
{
    [SerializeField]
    private MapLoader _mapLoader;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(_mapLoader);
    }
}