using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MeterTestSystemApi.Services;

/// <summary>
/// Describe a single IP probing.
/// </summary>
public class IPProbe : Probe
{
    /// <summary>
    /// Protocol to use.
    /// </summary>
    [NotNull, Required]
    public IPProbeProtocols Protocol { get; set; }

    /// <summary>
    /// IP endpoint to use.
    /// </summary>
    [NotNull, Required]
    public IPProbeEndPoint EndPoint { get; set; } = null!;

    /// <summary>
    /// Create a description for the probe.
    /// </summary>
    public override string ToString() => $"{EndPoint}[{Protocol}]";
}

