using System.Text.Json.Serialization;

namespace SourceApi.Model;

/// <summary>
/// All supported voltage amplifiers.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VoltageAuxiliaries
{
    /// <summary>
    /// 
    /// </summary>
    SVG150x00,

    /// <summary>
    /// 
    /// </summary>
    V210,

    /// <summary>
    /// 
    /// </summary>
    VU211x0,

    /// <summary>
    /// 
    /// </summary>
    VU211x1,

    /// <summary>
    /// 
    /// </summary>
    VU211x2,

    /// <summary>
    /// 
    /// </summary>
    VU220,

    /// <summary>
    /// 
    /// </summary>
    VU220x01,

    /// <summary>
    /// 
    /// </summary>
    VU220x02,

    /// <summary>
    /// 
    /// </summary>
    VU220x03,

    /// <summary>
    /// 
    /// </summary>
    VU220x04,

    /// <summary>
    /// 
    /// </summary>
    VU220x6,

    /// <summary>
    /// 
    /// </summary>
    VU221x0,

    /// <summary>
    /// 
    /// </summary>
    VU221x1,

    /// <summary>
    /// 
    /// </summary>
    VU221x2,

    /// <summary>
    /// 
    /// </summary>
    VU221x3,

    /// <summary>
    /// 
    /// </summary>
    VUI301,

    /// <summary>
    /// 
    /// </summary>
    VUI302,
}