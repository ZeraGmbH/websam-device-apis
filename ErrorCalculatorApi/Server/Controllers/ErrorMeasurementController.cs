using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ErrorCalculatorApi.Actions.Device;
using ErrorCalculatorApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ErrorCalculatorApi.Controllers;

/// <summary>
/// Controller to manage error calculators.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/ErrorCalculator/[controller]")]
public class ErrorMeasurementController : ControllerBase
{
    private readonly IErrorCalculator _device;

    /// <summary>
    /// Initialize a new controller.
    /// </summary>
    /// <param name="device">Serial port connected device to use.</param>
    public ErrorMeasurementController(IErrorCalculator device)
    {
        _device = device;
    }

    /// <summary>
    /// Configure the error measurement.
    /// </summary>
    /// <param name="meterConstant">Meter constant for the DUT - impulses per kWh.</param>
    /// <param name="impulses">Number of impulses.</param>
    [HttpPut]
    [SwaggerOperation(OperationId = "SetParameters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> SetParameters(double meterConstant, long impulses) =>
        Utils.SafeExecuteSerialPortCommand(() => _device.SetErrorMeasurementParameters(meterConstant, impulses));

    /// <summary>
    /// Retrieve the current status of the error measurement.
    /// </summary>
    /// <returns>Status information.</returns>
    [HttpGet]
    [SwaggerOperation(OperationId = "GetStatus")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ErrorMeasurementStatus>> GetStatus() =>
        Utils.SafeExecuteSerialPortCommand(_device.GetErrorStatus);

    /// <summary>
    /// Start a single error measurement.
    /// </summary>
    [HttpPost("StartSingle")]
    [SwaggerOperation(OperationId = "StartSingle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> StartSingle() =>
        Utils.SafeExecuteSerialPortCommand(() => _device.StartErrorMeasurement(false));

    /// <summary>
    /// Start a coninous error measurement.
    /// </summary>
    [HttpPost("StartContinuous")]
    [SwaggerOperation(OperationId = "StartContinuous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> StartContinuous() =>
        Utils.SafeExecuteSerialPortCommand(() => _device.StartErrorMeasurement(true));

    /// <summary>
    /// Abort the error measurement.
    /// </summary>
    [HttpPost("Abort")]
    [SwaggerOperation(OperationId = "Abort")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> Abort() =>
        Utils.SafeExecuteSerialPortCommand(_device.AbortErrorMeasurement);
}