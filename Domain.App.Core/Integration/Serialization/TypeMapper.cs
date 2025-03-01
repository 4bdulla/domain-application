using System.Reflection;

namespace Domain.App.Core.Integration.Serialization;

interface ISampleInterface
{
}

public static class TypeMapper
{
    private static readonly Dictionary<Type, string> TypeToNameMappings = new();
    private static readonly Dictionary<string, Type> NameToTypeMappings = new();


    static TypeMapper()
    {
        Type[] targetInterfaces = { typeof(ISampleInterface) };
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            RegisterTypesFromAssembly(assembly, targetInterfaces);
        }
    }


    public static Type GetTypeFromName(string typeName)
    {
        if (!NameToTypeMappings.TryGetValue(typeName, out Type type))
            throw new InvalidOperationException($"Unknown type discriminator '{typeName}' during deserialization.");

        return type;
    }

    public static string GetTypeName(Type type)
    {
        if (!TypeToNameMappings.TryGetValue(type, out string typeName))
            throw new InvalidOperationException("Type is not registered for serialization.");

        return typeName;
    }


    private static void RegisterTypesFromAssembly(Assembly assembly, Type[] targetInterfaces)
    {
        foreach (Type targetInterface in targetInterfaces)
        {
            if (!targetInterface.IsInterface)
                throw new ArgumentException($"{targetInterface.FullName} is not an interface.");

            Type[] types = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && targetInterface.IsAssignableFrom(type))
                .ToArray();

            foreach (Type type in types)
            {
                RegisterType(type);
            }
        }
    }

    private static void RegisterType(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        string typeName = type.Name;
        TypeToNameMappings[type] = typeName;
        NameToTypeMappings[typeName] = type;
    }
}