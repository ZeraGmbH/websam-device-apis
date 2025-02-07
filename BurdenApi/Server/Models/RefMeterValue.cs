using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ZERA.WebSam.Shared.Models.ReferenceMeter;
using ZERA.WebSam.Shared.DomainSpecific;

namespace BurdenApi.Models;

/// <summary>
/// An extended pair of values.
/// </summary>
public class RefMeterValue : GoalValue
{
    /// <summary>
    /// Full data of the measured phase.
    /// </summary>
    [NotNull, Required]
    public MeasuredLoadpointPhase Phase { get; set; } = new();

    /// <summary>
    /// Meaured frequency.
    /// </summary>
    public Frequency? Frequency { get; set; }

    /// <summary>
    /// Measured voltage range.
    /// </summary>
    [NotNull, Required]
    public Voltage? VoltageRange { get; set; }

    /// <summary>
    /// Measrued current range.
    /// </summary>
    [NotNull, Required]
    public Current? CurrentRange { get; set; }

    /// <summary>
    /// Silent convert from an internal measurement to the protocol representation.
    /// </summary>
    /// <param name="values">Measured values.</param>
    public static implicit operator RefMeterValue(RefMeterValueWithQuantity values) => new()
    {
        ApparentPower = values.ApparentPower,
        CurrentRange = values.CurrentRange,
        Frequency = values.Frequency,
        Phase = values.Phase,
        PowerFactor = values.PowerFactor,
        VoltageRange = values.VoltageRange,
    };
}