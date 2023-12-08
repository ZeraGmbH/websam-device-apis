using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using RefMeterApi.Models;
using SourceApi.Model;

namespace MeterTestSystemApi.Models;

/// <summary>
/// 
/// </summary>
public class MeterTestSystemCapabilities
{
    /// <summary>
    /// 
    /// </summary>
    [NotNull, Required]
    public List<VoltageAmplifiers> SupportedVoltageAmplifiers { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [NotNull, Required]
    public List<VoltageAuxiliaries> SupportedVoltageAuxiliaries { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [NotNull, Required]
    public List<CurrentAmplifiers> SupportedCurrentAmplifiers { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [NotNull, Required]
    public List<CurrentAuxiliaries> SupportedCurrentAuxiliaries { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [NotNull, Required]
    public List<ReferenceMeters> SupportedReferenceMeters { get; set; } = new();
}