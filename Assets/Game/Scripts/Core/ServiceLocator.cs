using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services =
        new Dictionary<Type, object>();

    // Overwrites whatever was registered for T before.
    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
    }

    // First instance wins; returns false when T is already registered.
    public static bool TryRegister<T>(T service) where T : class
    {
        if (services.ContainsKey(typeof(T)))
            return false;

        services[typeof(T)] = service;

        return true;
    }

    public static T Get<T>() where T : class
    {
        return services.TryGetValue(typeof(T), out object service)
            ? service as T
            : null;
    }

    // Only clears the slot while it still points at this exact instance.
    public static void Unregister<T>(T service) where T : class
    {
        if (services.TryGetValue(typeof(T), out object existing) &&
            ReferenceEquals(existing, service))
        {
            services.Remove(typeof(T));
        }
    }
}
