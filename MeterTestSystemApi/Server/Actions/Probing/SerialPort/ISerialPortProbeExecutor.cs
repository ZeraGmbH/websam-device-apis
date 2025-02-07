using SerialPortProxy;

namespace MeterTestSystemApi.Actions.Probing.SerialPort;

/// <summary>
/// Interface for probing over a serial port.
/// </summary>
public interface ISerialPortProbeExecutor
{
    /// <summary>
    /// Set to enable the serial port connection background reader.
    /// </summary>
    bool EnableReader { get; }

    /// <summary>
    /// Adjust the serial port options if necessary.
    /// </summary>
    /// <param name="options">Options to use.</param>
    void AdjustOptions(SerialPortOptions options);

    /// <summary>
    /// Run a single probing algorithm.
    /// </summary>
    /// <param name="connection">Connection to use.</param>
    /// <returns>Error message or empty.</returns>
    Task<ProbeInfo> ExecuteAsync(ISerialPortConnection connection);
}