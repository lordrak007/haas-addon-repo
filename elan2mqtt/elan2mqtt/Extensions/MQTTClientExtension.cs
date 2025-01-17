using elan2mqtt.Model.eLan;
using HomeAssistantDiscoveryNet;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MQTTnet.Extensions;

internal static class ObjectExtensions
{
    public static TObject DumpToConsole<TObject>(this TObject @object)
    {
        var output = "NULL";
        if (@object != null)
        {
            output = JsonSerializer.Serialize(@object, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        Console.WriteLine($"[{@object?.GetType().Name}]:\r\n{output}");
        return @object;
    }

    public static string Linearize(this string text)
    {
        return text.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("  ","");
    }

}

public static class MqttDiscoveryConfigExtensions
{
    public static string ToJson<T>(this T config, JsonSerializerContext? ctx = null) where T : MqttDiscoveryConfig
    {
        ctx ??= MqttDiscoveryJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(config.GetType()) ?? throw new InvalidOperationException("The JsonTypeInfo for " + config.GetType().FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.Serialize(config, jsonTypeInfo);
    }

    public static byte[] ToJsonBxtes<T>(this T config, JsonSerializerContext? ctx = null) where T : MqttDiscoveryConfig
    {
        ctx ??= MqttDiscoveryJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(config.GetType()) ?? throw new InvalidOperationException("The JsonTypeInfo for " + config.GetType().FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.SerializeToUtf8Bytes(config, jsonTypeInfo);
    }

    public static T? FromJson<T>(this string json, JsonSerializerContext? ctx = null) where T : MqttDiscoveryConfig
    {
        ctx ??= MqttDiscoveryJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(typeof(T)) ?? throw new InvalidOperationException("The JsonTypeInfo for " + typeof(T).FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.Deserialize<T>(json, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)jsonTypeInfo);
    }

    public static T? FromJsonBytes<T>(this byte[] jsonBytes, JsonSerializerContext? ctx = null) where T : MqttDiscoveryConfig
    {
        ctx ??= MqttDiscoveryJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(typeof(T)) ?? throw new InvalidOperationException("The JsonTypeInfo for " + typeof(T).FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.Deserialize<T>(jsonBytes, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)jsonTypeInfo);
    }
}


[JsonSerializable(typeof(ActionTypeFromJson))]
[JsonSerializable(typeof(ActionsInfoFromJson))]
[JsonSerializable(typeof(DeviceInfoFromJson))]
[JsonSerializable(typeof(ElkoEpDevFromJson))]
//[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(ElanDevices))]
[JsonSerializable(typeof(IList<string>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<object>))]
[JsonSerializable(typeof(Array))]
[JsonSourceGenerationOptions(
WriteIndented = true,
PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ElDevJsonContext : JsonSerializerContext
{

}


public static class ElDevExtensions
{
    public static string ToJson<T>(this T config, JsonSerializerContext? ctx = null) where T : ElkoEpDevFromJson
    {
        ctx ??= ElDevJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(config.GetType()) ?? throw new InvalidOperationException("The JsonTypeInfo for " + config.GetType().FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.Serialize(config, jsonTypeInfo);
    }

    public static ElkoEpDevFromJson? FromJson<T>(this string json, JsonSerializerContext? ctx = null) where T : ElkoEpDevFromJson
    {
        ctx ??= ElDevJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(typeof(T)) ?? throw new InvalidOperationException("The JsonTypeInfo for " + typeof(T).FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.Deserialize<T>(json, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)jsonTypeInfo);
    }

    public static T? FromJsonBytes<T>(this byte[] jsonBytes, JsonSerializerContext? ctx = null) where T : ElkoEpDevFromJson
    {
        ctx ??= ElDevJsonContext.Default;
        var jsonTypeInfo = ctx.GetTypeInfo(typeof(T)) ?? throw new InvalidOperationException("The JsonTypeInfo for " + typeof(T).FullName + " was not found in the provided JsonSerializerContext. If you have a custom Discovery Document you might need to provide your own JsonSerializerContext");
        return JsonSerializer.Deserialize<T>(jsonBytes, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<T>)jsonTypeInfo);
    }




}