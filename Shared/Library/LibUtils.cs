using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;
using SharedLibrary.DomainSpecific;

namespace SharedLibrary;

/// <summary>
/// 
/// </summary>
public static class LibUtils
{
    /// <summary>
    /// Configure serializer to generate camel casing.
    /// </summary>
    public static readonly JsonSerializerOptions JsonSettings = new()
    {
        Converters = { new DomainSpecificNumber.Converter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Copys values of object, not its reference
    /// </summary>
    /// <typeparam name="T">Any type of object</typeparam>
    /// <param name="self">object to clone</param>
    /// <returns>a clone of the object</returns>
    public static T DeepCopyAs<T>(object? self) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(self, JsonSettings), JsonSettings)!;

    /// <summary>
    /// Copys values of object, not its reference
    /// </summary>
    /// <typeparam name="T">Any type of object</typeparam>
    /// <param name="self">object to clone</param>
    /// <returns>a clone of the object</returns>
    public static T DeepCopy<T>(T self) => DeepCopyAs<T>(self);

    /// <summary>
    /// Extract a scalar value from a JsonElement.
    /// </summary>
    /// <param name="json">As parsed with the System.Text.Json library.</param>
    /// <returns>The value.</returns>
    public static object? ToJsonScalar(this JsonElement json)
        => json.ValueKind switch
        {
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Number => json.Deserialize<double>(),
            JsonValueKind.String => json.Deserialize<string>(),
            JsonValueKind.True => true,
            _ => (object?)json,
        };

    /// <summary>
    /// Extract a scalar value from a JsonElement.
    /// </summary>
    /// <param name="json">As parsed with the System.Text.Json library.</param>
    /// <returns>The value.</returns>
    public static object? ToJsonScalar(this JsonNode json)
        => json.GetValueKind() switch
        {
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Number => json.GetValue<double>(),
            JsonValueKind.String => json.GetValue<string>(),
            JsonValueKind.True => true,
            _ => (object?)json,
        };

    /// <summary>
    /// Deserialize a JSON element to an instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance.</typeparam>
    /// <param name="node">Element to deserialize.</param>
    /// <returns>Instance - may be null if JSON element represents null.</returns>
    public static T? DefaultDeserialize<T>(this JsonNode node) => JsonSerializer.Deserialize<T>(node, JsonSettings);

    /// <summary>
    /// Deserialize a JSON element to an instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance.</typeparam>
    /// <param name="element">Element to deserialize.</param>
    /// <returns>Instance - may be null if JSON element represents null.</returns>
    public static T? DefaultDeserialize<T>(this JsonElement element) => JsonSerializer.Deserialize<T>(element, JsonSettings);

    /// <summary>
    /// Deserialize a dynamic object to an instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance.</typeparam>
    /// <param name="obj">Element to deserialize.</param>
    /// <returns>Instance - may be null if the object is null.</returns>
    public static T? DefaultDeserialize<T>(this ExpandoObject obj) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj, JsonSettings), JsonSettings);
}