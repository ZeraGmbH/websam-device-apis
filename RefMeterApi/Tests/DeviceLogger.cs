using Microsoft.Extensions.Logging;
using RefMeterApi.Actions.Device;

namespace RefMeterApiTests;

public class DeviceLogger : ILogger<SerialPortMTRefMeter>
{
    class Scope : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new Scope();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        throw new ArgumentException(formatter(state, exception));
}
