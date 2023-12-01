using ErrorCalculatorApi.Models;

namespace ErrorCalculatorApi.Actions.Device;

/// <summary>
/// API for any error calculator device.
/// </summary>
public interface IErrorCalculator
{
    /// <summary>
    /// Configure the error measurement.
    /// </summary>
    /// <param name="meterConstant">The meter constant of the device under test - impulses per kWh.</param>
    /// <param name="impulses"></param>
    Task SetErrorMeasurementParameters(double meterConstant, long impulses);

    /// <summary>
    /// Start the error measurement.
    /// </summary>
    /// <param name="continuous">Unset for a single measurement.</param>
    Task StartErrorMeasurement(bool continuous);

    /// <summary>
    /// Terminate the error measurement.
    /// </summary>
    Task AbortErrorMeasurement();

    /// <summary>
    /// Report the current status of the error measurement.
    /// </summary>
    /// <returns>The current status.</returns>
    Task<ErrorMeasurementStatus> GetErrorStatus();

}