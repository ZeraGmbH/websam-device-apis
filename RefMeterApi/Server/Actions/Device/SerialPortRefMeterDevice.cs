using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RefMeterApi.Models;
using SerialPortProxy;

namespace RefMeterApi.Actions.Device;

/// <summary>
/// Handle all requests to a device.
/// </summary>
public class SerialPortRefMeterDevice : IRefMeterDevice
{
    private static readonly Dictionary<string, MeasurementModes> SupportedModes = new() {
        {"2WA", MeasurementModes.TwoWireActivePower},
        {"2WAP", MeasurementModes.TwoWireApparentPower},
        {"2WR", MeasurementModes.TwoWireReactivePower},
        {"3WA", MeasurementModes.ThreeWireActivePower},
        {"3WAP", MeasurementModes.ThreeWireApparentPower},
        {"3WR", MeasurementModes.ThreeWireReactivePower},
        {"3WRCA", MeasurementModes.ThreeWireReactivePowerCrossConectedA},
        {"3WRCB", MeasurementModes.ThreeWireReactivePowerCrossConectedB},
        {"4WA", MeasurementModes.FourWireActivePower},
        {"4WAP", MeasurementModes.FourWireApparentPower},
        {"4WR", MeasurementModes.FourWireReactivePower},
        {"4WRC", MeasurementModes.FourWireReactivePowerCrossConected},
    };

    private static readonly Regex ActualValueReg = new Regex("^(\\d{1,3});(.+)$");

    private static readonly Regex MeasurementModeReg = new Regex("^(\\d{1,3});([^;]+);(.+)$");

    private readonly SerialPortConnection _device;

    private readonly ILogger<SerialPortRefMeterDevice> _logger;

    /* Outstanding AME request - only works properly if the device instance is a singleton. */
    private readonly ResponseShare<MeasureOutput> _actualValues;

    /// <summary>
    /// Initialize device manager.
    /// </summary>
    /// <param name="device">Service to access the current serial port.</param>
    /// <param name="logger">Logging service for this device type.</param>
    public SerialPortRefMeterDevice(SerialPortConnection device, ILogger<SerialPortRefMeterDevice> logger)
    {
        _device = device;
        _logger = logger;

        /* Setup caches for shared request results. */
        _actualValues = new(CreateActualValueRequest);
    }

    /// <inheritdoc/>
    public Task<MeasureOutput> GetActualValues() => _actualValues.Execute();

    /// <inheritdoc/>
    public async Task<MeasurementModes[]> GetMeasurementModes()
    {
        /* Execute the request and get the answer from the device. */
        var replies = await _device.Execute(
            SerialPortRequest.Create("AML", "AMLACK")
        )[0];

        /* Make sure this is an AME reply sequence. */
        if (replies[^1] != "AMLACK")
            throw new ArgumentException("missing AMLACK", nameof(replies));

        /* Prepare response with three phases. */
        var response = new List<MeasurementModes>();

        for (var i = 0; i < replies.Length - 1; i++)
        {
            /* Chck for a value with index. */
            var reply = replies[i];
            var match = MeasurementModeReg.Match(reply);

            if (!match.Success)
            {
                /* Report bad reply and ignore it. */
                _logger.LogWarning($"bad reply {reply}");

                continue;
            }

            /* Get the english short name. */
            if (SupportedModes.TryGetValue(match.Groups[2].Value, out var mode))
                response.Add(mode);
        }

        return response.ToArray();
    }

    /// <summary>
    /// Begin reading the actual values - this may take some time.
    /// </summary>
    /// <returns>Task reading the actual values.</returns>
    /// <exception cref="ArgumentException">Reply from the device was not recognized.</exception>
    private async Task<MeasureOutput> CreateActualValueRequest()
    {
        /* Execute the request and get the answer from the device. */
        var replies = await _device.Execute(
            SerialPortRequest.Create("ATI01", "ATIACK"),
            SerialPortRequest.Create("AME", "AMEACK")
        )[1];

        /* Make sure this is an AME reply sequence. */
        if (replies[^1] != "AMEACK")
            throw new ArgumentException("missing AMEACK", nameof(replies));

        /* Prepare response with three phases. */
        var response = new MeasureOutput
        {
            Phases = { new MeasureOutputPhase(), new MeasureOutputPhase(), new MeasureOutputPhase(), }
        };

        for (var i = 0; i < replies.Length - 1; i++)
        {
            /* Chck for a value with index. */
            var reply = replies[i];
            var match = ActualValueReg.Match(reply);

            if (!match.Success)
            {
                /* Report bad reply and ignore it. */
                _logger.LogWarning($"bad reply {reply}");

                continue;
            }

            /* Decode index and value - make sure that parsing is not messed by local operating system regional settings. */
            int index;
            double value = 0;

            try
            {
                index = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);

                if (index != 27)
                    value = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                /* Report bad number and ignore reply. */
                _logger.LogWarning($"invalid number in reply {reply}");

                continue;
            }

            if (index < 0)
            {
                /* Report bad number and ignore reply. */
                _logger.LogWarning($"bad reply {reply}");

                continue;
            }

            /* Copy value to the appropriate field. */
            switch (index)
            {
                case 0:
                    response.Phases[0].Voltage = value;
                    break;
                case 1:
                    response.Phases[1].Voltage = value;
                    break;
                case 2:
                    response.Phases[2].Voltage = value;
                    break;
                case 3:
                    response.Phases[0].Current = value;
                    break;
                case 4:
                    response.Phases[1].Current = value;
                    break;
                case 5:
                    response.Phases[2].Current = value;
                    break;
                case 6:
                    response.Phases[0].AngleVoltage = value;
                    break;
                case 7:
                    response.Phases[1].AngleVoltage = value;
                    break;
                case 8:
                    response.Phases[2].AngleVoltage = value;
                    break;
                case 9:
                    response.Phases[0].AngleCurrent = value;
                    break;
                case 10:
                    response.Phases[1].AngleCurrent = value;
                    break;
                case 11:
                    response.Phases[2].AngleCurrent = value;
                    break;
                case 12:
                    response.Phases[0].PowerFactor = value;
                    break;
                case 13:
                    response.Phases[1].PowerFactor = value;
                    break;
                case 14:
                    response.Phases[2].PowerFactor = value;
                    break;
                case 15:
                    response.Phases[0].ActivePower = value;
                    break;
                case 16:
                    response.Phases[1].ActivePower = value;
                    break;
                case 17:
                    response.Phases[2].ActivePower = value;
                    break;
                case 18:
                    response.Phases[0].ReactivePower = value;
                    break;
                case 19:
                    response.Phases[1].ReactivePower = value;
                    break;
                case 20:
                    response.Phases[2].ReactivePower = value;
                    break;
                case 21:
                    response.Phases[0].ApparentPower = value;
                    break;
                case 22:
                    response.Phases[1].ApparentPower = value;
                    break;
                case 23:
                    response.Phases[2].ApparentPower = value;
                    break;
                case 24:
                    response.ActivePower = value;
                    break;
                case 25:
                    response.ReactivePower = value;
                    break;
                case 26:
                    response.ApparentPower = value;
                    break;
                case 27:
                    response.PhaseOrder = match.Groups[2].Value;
                    break;
                case 28:
                    response.Frequency = value;
                    break;
            }
        }

        return response;
    }
}
