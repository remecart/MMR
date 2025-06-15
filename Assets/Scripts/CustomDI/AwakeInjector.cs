using System.Reflection;
using UnityEngine;
using VContainer.Unity;

public static class AwakeInjector
{
    public static void InjectInto(MonoBehaviour mono, LifetimeScope scope)
    {
        var fields = mono.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (var field in fields)
        {
            if (field.GetCustomAttribute<AwakeInjectAttribute>() != null)
            {
                var value = scope.Container.Resolve(field.FieldType);
                field.SetValue(mono, value);
            }
        }
    }
}