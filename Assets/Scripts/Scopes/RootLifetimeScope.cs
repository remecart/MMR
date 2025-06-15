using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Auto Register all game objects in the scene
        autoInjectGameObjects = FindObjectsOfType<GameObject>()
            .Where(x => x.GetComponent<LifetimeScope>() == null)
            .Where(x => x.GetComponents<MonoBehaviour>().Length > 0)
            .ToList();

        var configLoader = new ConfigLoader();
        builder.RegisterInstance(configLoader)
            .AsSelf()
            .AsImplementedInterfaces();

        foreach (var config in configLoader.GetAll())
        {
            builder.RegisterInstance(config)
                .AsSelf();
        }

        builder.Register<GuiSettingsGenerator>(Lifetime.Singleton)
            .AsSelf();
    }
}