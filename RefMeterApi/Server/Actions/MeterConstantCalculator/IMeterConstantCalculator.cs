using RefMeterApi.Models;
using SharedLibrary.DomainSpecific;

namespace RefMeterApi.Actions.MeterConstantCalculator;

/// <summary>
/// Interface to calculate meter constants for various reference 
/// meters.
/// </summary>
public interface IMeterConstantCalculator
{
    /// <summary>
    /// Given the electrical ranges calculate the meter constant.
    /// </summary>
    /// <param name="meter">The type of the meter to use.</param>
    /// <param name="frequency">Nominal frequency in Hz.</param>
    /// <param name="mode">The measurement mode.</param>
    /// <param name="voltageRange">Voltage range in V.</param>
    /// <param name="currentRange">Current range in C.</param>
    /// <returns>The meter constant.</returns>
    MeterConstant GetMeterConstant(ReferenceMeters meter, double frequency, MeasurementModes mode, double voltageRange, double currentRange);
}