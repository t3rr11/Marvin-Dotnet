using System.Text.Json;
using Unleash.Serialization;

namespace Marvin.ClanScannerServer.Services;

public class UnleashJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public UnleashJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }
    
    public T? Deserialize<T>(Stream stream)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(stream, _jsonSerializerOptions);
        }
        catch
        {
            return default;
        }
    }

    public void Serialize<T>(Stream stream, T instance)
    {
        JsonSerializer.Serialize<T>(stream, instance, _jsonSerializerOptions);
    }
}