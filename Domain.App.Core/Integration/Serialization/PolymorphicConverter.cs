using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Domain.App.Core.Integration.Serialization;

public class PolymorphicConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsInterface ||
            typeToConvert.IsArray && typeToConvert.GetElementType()?.IsInterface == true ||
            typeToConvert.IsGenericType &&
            typeof(IEnumerable<>).IsAssignableFrom(typeToConvert.GetGenericTypeDefinition()) &&
            typeToConvert.GenericTypeArguments[0].IsInterface;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();

                return;

            // Handle arrays and collections
            case Array array:
                writer.WriteStartArray();

                foreach (object item in array) WriteSingleItem(writer, item, options);

                writer.WriteEndArray();

                break;

            case IEnumerable<object> collection:
                writer.WriteStartArray();

                foreach (object item in collection) WriteSingleItem(writer, item, options);

                writer.WriteEndArray();

                break;

            default:
                // Handle single object serialization
                WriteSingleItem(writer, value, options);

                break;
        }
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsArray) return ReadArray(ref reader, typeToConvert, options);

        return typeToConvert.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(typeToConvert.GetGenericTypeDefinition())
            ? ReadGenericCollection(ref reader, typeToConvert, options)
            : ReadSingleItem(ref reader, options)!;
    }


    private static void WriteSingleItem(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonObject jsonObject = JsonSerializer.SerializeToNode(value, value.GetType(), options)!.AsObject();
        jsonObject.Add("$type", TypeMapper.GetTypeName(value.GetType()));
        jsonObject.WriteTo(writer);
    }

    private static object ReadArray(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GetElementType()!;
        var items = new List<object>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            items.Add(ReadSingleItem(ref reader, options)!);
        }

        var array = Array.CreateInstance(elementType, items.Count);

        // Manually copy items to the strongly-typed array
        for (var i = 0; i < items.Count; i++)
        {
            array.SetValue(items[i], i);
        }

        return array;
    }

    private static object ReadGenericCollection(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GenericTypeArguments[0];
        var items = new List<object>();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            items.Add(ReadSingleItem(ref reader, options)!);
        }

        Type collectionType = typeof(List<>).MakeGenericType(elementType);
        var collection = (IList)Activator.CreateInstance(collectionType)!;
        foreach (object item in items) collection.Add(item);

        return collection;
    }


    private static object ReadSingleItem(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        JsonObject jsonObject = JsonNode.Parse(ref reader)!.AsObject();
        var typeName = jsonObject["$type"]?.ToString();

        if (typeName == null)
            throw new JsonException("Type discriminator '$type' is missing.");

        Type actualType = TypeMapper.GetTypeFromName(typeName);

        return JsonSerializer.Deserialize(jsonObject.ToJsonString(), actualType, options);
    }
}