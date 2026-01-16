using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedKernel.ServiceBus;

/// <summary>
/// Provides consistent JSON serializer options for Service Bus messages
/// </summary>
public static class JsonSerializerOptionsProvider
{
    public static JsonSerializerOptions Default { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

