using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.App.Core.Integration.Serialization;

namespace Domain.App.Core.Integration;

public static class JsonHandling
{
    /// <summary>
    /// Used for configuring incoming requests serialization options
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new PolymorphicConverter() }
    };
}