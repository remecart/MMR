using System;
using UnityEngine;
using VContainer;

public class ChangeTracker : MonoBehaviour
{
    [Inject]
    private readonly ConfigLoader _configLoader;

    private void Awake()
    {
        if (_configLoader == null)
        {
            Debug.LogError("ConfigLoader is not injected. Please ensure it is set up in the VContainer configuration.");
        }
    }

    private void FixedUpdate()
    {
        foreach (var config in _configLoader.GetAll())
        {
            var type = config.GetType();
            if (_configLoader.IsChanged(type))
            {
                _configLoader.SaveChanges(type);
            }
        }
    }

    public void RevertChanges<T>() where T : class, IConfig
    {
        var type = typeof(T);
        if (_configLoader.IsChanged(type))
        {
            _configLoader.RevertChanges(type);
        }
    }

    public void RevertChanges(Type type)
    {
        if (!typeof(IConfig).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type {type.Name} does not implement IConfig.");
        }

        if (_configLoader.IsChanged(type))
        {
            _configLoader.RevertChanges(type);
        }
    }
}