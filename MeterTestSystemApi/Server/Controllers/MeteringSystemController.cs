using MeterTestSystemApi.Actions.Device;
using MeterTestSystemApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SerialPortProxy;
using ZERA.WebSam.Shared.Models.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace MeterTestSystemApi.Controllers;

/// <summary>
/// Controller to access the current meter test system.
/// </summary>
/// <remarks>
/// Initialize a new meter test system controller.
/// </remarks>
/// <param name="device">Meter test system to use.</param>
/// <param name="interfaceLogger"></param>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class MeterTestSystemController(IMeterTestSystem device, IInterfaceLogger interfaceLogger) : ControllerBase
{
    /// <summary>
    /// The meter test system to use during this request.
    /// </summary>
    private readonly IMeterTestSystem _device = device;

    /// <summary>
    /// Report the capabilities of the current meter test system.
    /// </summary>
    /// <returns>May be null if the meter test system does not allow configuration.</returns>
    [HttpGet]
    [SwaggerOperation(OperationId = "GetMeterTestSystemCapabilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<MeterTestSystemCapabilities>> GetCapabilities() =>
        ActionResultMapper.SafeExecuteSerialPortCommand(() => _device.GetCapabilities(interfaceLogger));

    /// <summary>
    /// Set the physical configuration of a meter test system.
    /// </summary>
    [HttpPut("AmplifiersAndReferenceMeter")]
    [SwaggerOperation(OperationId = "SetAmplifiersAndReferenceMeter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult> SetAmplifiersAndReferenceMeter([FromBody] AmplifiersAndReferenceMeter request) =>
        ActionResultMapper.SafeExecuteSerialPortCommand(() => _device.SetAmplifiersAndReferenceMeter(interfaceLogger, request));

    /// <summary>
    /// Report the current pysical configuration of the meter test system.
    /// </summary>
    /// <returns></returns>
    [HttpGet("AmplifiersAndReferenceMeter")]
    [SwaggerOperation(OperationId = "GetAmplifiersAndReferenceMeter")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<AmplifiersAndReferenceMeter>> GetAmplifiersAndReferenceMeter() =>
        ActionResultMapper.SafeExecuteSerialPortCommand(() => Task.FromResult(_device.GetAmplifiersAndReferenceMeter(interfaceLogger)));

    /// <summary>
    /// Read the firmware from the metering system.
    /// </summary>
    /// <returns>Firmware version of the metering system.</returns>
    [HttpGet("FirmwareVersion")]
    [SwaggerOperation(OperationId = "GetFirmwareVersion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<MeterTestSystemFirmwareVersion>> GetFirmwareVersion() =>
        ActionResultMapper.SafeExecuteSerialPortCommand(() => _device.GetFirmwareVersion(interfaceLogger));

    /// <summary>
    /// Read the current error conditions of the meter test system.
    /// </summary>
    /// <returns>All current error conditions.</returns>
    [HttpGet("ErrorConditions")]
    [SwaggerOperation(OperationId = "GetErrorConditions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    [ProducesResponseType(StatusCodes.Status408RequestTimeout)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<ErrorConditions>> GetErrorConditions() =>
        ActionResultMapper.SafeExecuteSerialPortCommand(() => _device.GetErrorConditions(interfaceLogger));
}
