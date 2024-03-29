using System.Text.Json.Serialization;

namespace SourceApi.Model
{
    /// <summary>
    /// Describes how the frequency should be generated.
    /// </summary>
    [Serializable]
    public class Frequency
    {
        /// <summary>
        /// The mode of how the frequency should be generated
        /// </summary>
        public FrequencyMode Mode { get; set; }

        /// <summary>
        /// If applicable, the value of the frequency in Hertz.
        /// </summary>
        public double Value { get; set; }
    }

    /// <summary>
    /// Diiferent modi operandi for how to generate the frequency.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FrequencyMode
    {
        /// <summary>
        /// The frequency is generated by the source on the basis of a quartz clock.
        /// </summary>
        SYNTHETIC = 0,
        /// <summary>
        /// The frequency is specified by the power supply.
        /// </summary>
        GRID_SYNCRONOUS = 1,
        // <summary>
        // This is a DC-Loadpoint
        // </summary>
        //DIRECT_CURRENT = 2
    }
}