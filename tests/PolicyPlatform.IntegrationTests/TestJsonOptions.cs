using System.Text.Json;
using System.Text.Json.Serialization;

namespace PolicyPlatform.IntegrationTests;

/// <summary>The API serializes enums as strings (see Program.cs's JsonStringEnumConverter
/// registration) — HttpClient's JSON extensions don't know that by default, so tests need
/// the matching options to deserialize responses.</summary>
public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
