using System.Text.Json.Serialization;

namespace SharedLibrary.Models.Logging;

/// <summary>
/// Information on the payload of a communication.
/// </summary>
public class InterfaceLogPayload
{
    /// <summary>
    /// Payload of the data.
    /// </summary>
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Encoding of the payload.
    /// </summary>
    public InterfaceLogPayloadEncodings Encoding { get; set; }

    /// <summary>
    /// Semantic type of the payload allowing reporting
    /// tools to display the information appropriatly.
    /// </summary>
    public string PayloadType { get; set; } = null!;

    /// <summary>
    /// Error received during communication.
    /// </summary>
    public string? TransferException { get; set; }

    /// <summary>
    /// Set if the there is no payload at all.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Encoding == InterfaceLogPayloadEncodings.Raw && Payload == null && PayloadType == null && TransferException == null;
}