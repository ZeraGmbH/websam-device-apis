using MeterTestSystemApi.Actions.Probing.SerialPort;
using SerialPortProxy;
using ZERA.WebSam.Shared.Models.Logging;

namespace MeterTestSystemApi.Actions.Probing.ESxB;

/// <summary>
/// Probe for a burden connected to a serial port.
/// </summary>
public class ESxBSerialPortProbing(IInterfaceLogger logger) : ISerialPortProbeExecutor
{
    /// <inheritdoc/>
    public bool EnableReader => true;

    /// <inheritdoc/>
    public void AdjustOptions(SerialPortOptions options) { }

    /// <inheritdoc/>
    public async Task<ProbeInfo> ExecuteAsync(ISerialPortConnection connection)
    {
        var executor = connection.CreateExecutor(InterfaceLogSourceTypes.Burden, "probe");

        var reply = await executor.ExecuteAsync(logger, SerialPortRequest.Create("AV", "AVACK"))[0];

        if (reply.Length < 3) return new() { Succeeded = false, Message = "invalid reply" };

        return new() { Message = $"ESxB Version {reply[^3]}", Succeeded = true };
    }
}