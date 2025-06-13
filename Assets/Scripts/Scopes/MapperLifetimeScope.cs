using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MapperLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var mapLoader = FindObjectOfType<MapLoader>();
        builder.RegisterInstance<MapLoader>(mapLoader);
        builder.RegisterComponentOnNewGameObject<Test>(Lifetime.Singleton, "a");
        
        
        Debug.Log("aa " + mapLoader);
    }
}
