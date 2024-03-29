using System.Text.Json.Serialization;

namespace SharedLibrary.Models;

/// <summary>
/// Supported types of the meter tests system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeterTestSystemTypes
{
    /// <summary>
    /// MT786 like.
    /// </summary>
    MT786 = 0,

    /// <summary>
    /// FG30x like.
    /// </summary>
    FG30x = 1,

    /// <summary>
    /// Internal AC mock implementation for development and testing
    /// purposes.
    /// </summary>
    ACMock = 2,

    /// <summary>
    /// REST connection to components of a meter test system.
    /// </summary>
    REST = 3,

    /// <summary>
    /// Internal DC mock implementation for development and testing
    /// purposes.
    /// </summary>
    DCMock = 4,
}