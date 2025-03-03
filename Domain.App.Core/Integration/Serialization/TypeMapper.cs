using System.Reflection;


namespace Domain.App.Core.Integration.Serialization;

internal interface ISampleInterface;


public static class TypeMapper
{
    private static readonly Dictionary<Type, string> _typeToNameMappings = new();
    private static readonly Dictionary<string, Type> _nameToTypeMappings = new();


    static TypeMapper()
    {
        Type[] targetInterfaces = [typeof(ISampleInterface)];
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            RegisterTypesFromAssembly(assembly, targetInterfaces);
        }
    }


    public static Type GetTypeFromName(string typeName)
    {
        if (!_nameToTypeMappings.TryGetValue(typeName, out Type type))
            throw new InvalidOperationException($"Unknown type discriminator '{typeName}' during deserialization.");

        return type;
    }

    public static string GetTypeName(Type type)
    {
        if (!_typeToNameMappings.TryGetValue(type, out string typeName))
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
        ArgumentNullException.ThrowIfNull(type);

        string typeName = type.Name;
        _typeToNameMappings[type] = typeName;
        _nameToTypeMappings[typeName] = type;
    }
}