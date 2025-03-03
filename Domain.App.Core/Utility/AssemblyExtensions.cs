using System.Reflection;


namespace Domain.App.Core.Utility;

public static class AssemblyExtensions
{
    public static IEnumerable<Type> GetTypesByAttribute<TAttribute>(this Assembly[] assemblies, Predicate<Type> predicate = null)
    where TAttribute : Attribute => assemblies.SelectMany(assembly => assembly.GetTypesByAttribute<TAttribute>(predicate));

    private static IEnumerable<Type> GetTypesByAttribute<TAttribute>(this Assembly assembly, Predicate<Type> predicate = null)
    where TAttribute : Attribute
    {
        IEnumerable<Type> types = assembly.GetTypes().Where(t => t.GetCustomAttribute<TAttribute>() is not null);

        if (predicate is not null)
        {
            types = types.Where(predicate.Invoke);
        }

        return types;
    }

    public static Type[] GetGenericArgumentsFromImplementedInterfaces(this Type type, Type genericTypeToSearch)
    {
        return type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericTypeToSearch)!
            .GetGenericArguments();
    }
}