using WebSamDeviceApis.Model;

namespace WebSamDeviceApis.Actions.SerialPort;

/// <summary>
/// 
/// </summary>
public static class CapabilitiesMap
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly SourceCapabilities Default = new()
    {
        Phases = new() {
                    new() {
                        Voltage = new(20, 500, 0.001),
                        Current = new(0, 120, 0.01)
                    },
                    new() {
                        Voltage = new(20, 500, 0.001),
                        Current = new(0, 120, 0.01)
                    },
                    new() {
                        Voltage = new(20, 500, 0.001),
                        Current = new(0, 120, 0.01)
                    }
                },
        FrequencyRanges = new() {
                    new(45, 65, 0.01, FrequencyMode.SYNTHETIC)
                }
    };

    private static readonly Dictionary<string, SourceCapabilities> ByModel = new()
        {
            { "MT786", Default },
            { "MT793", Default },
        };

    public static SourceCapabilities GetCapabilitiesByModel(string modelName)
    {
        if (ByModel.TryGetValue(modelName, out var capabilities))
            return capabilities;

        throw new ArgumentException($"Unknown model {modelName}");
    }
}
