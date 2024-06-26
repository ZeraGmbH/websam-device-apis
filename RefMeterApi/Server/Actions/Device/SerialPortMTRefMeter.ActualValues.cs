using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RefMeterApi.Models;
using SerialPortProxy;
using SharedLibrary;
using SharedLibrary.Models.Logging;
using SourceApi.Model;

namespace RefMeterApi.Actions.Device;

partial class SerialPortMTRefMeter
{
    private static readonly Regex ActualValueReg = new(@"^(\d{1,3});(.+)$");

    /* Outstanding AME request - only works properly if the device instance is a singleton. */
    private readonly ResponseShare<MeasuredLoadpoint, IInterfaceLogger> _actualValues;

    /// <inheritdoc/>
    public async Task<MeasuredLoadpoint> GetActualValues(IInterfaceLogger logger, int firstActiveCurrentPhase = -1)
        => Utils.ConvertFromDINtoIEC(LibUtils.DeepCopy(await _actualValues.Execute(logger)), firstActiveCurrentPhase);

    /// <summary>
    /// Begin reading the actual values - this may take some time.
    /// </summary>
    /// <returns>Task reading the actual values.</returns>
    /// <exception cref="ArgumentException">Reply from the device was not recognized.</exception>
    private async Task<MeasuredLoadpoint> CreateActualValueRequest(IInterfaceLogger logger)
    {
        /* Execute the request and get the answer from the device. */
        var replies = await _device.Execute(
            logger,
            SerialPortRequest.Create("ATI01", "ATIACK"),
            SerialPortRequest.Create("AME", "AMEACK")
        )[1];

        /* Prepare response with three phases. */
        var response = new MeasuredLoadpoint
        {
            Phases = {
                new MeasuredLoadpointPhase {
                    Current = new ElectricalQuantity { AcComponent = new() } ,
                    Voltage = new ElectricalQuantity { AcComponent = new() }
                },
                new MeasuredLoadpointPhase {
                    Current = new ElectricalQuantity { AcComponent = new() } ,
                    Voltage = new ElectricalQuantity { AcComponent = new() }
                },
                new MeasuredLoadpointPhase {
                    Current = new ElectricalQuantity { AcComponent = new() } ,
                    Voltage = new ElectricalQuantity { AcComponent = new() }
                },
            }
        };

        for (var i = 0; i < replies.Length - 1; i++)
        {
            /* Chck for a value with index. */
            var reply = replies[i];
            var match = ActualValueReg.Match(reply);

            if (!match.Success)
            {
                /* Report bad reply and ignore it. */
                _logger.LogWarning("bad reply {Reply}", reply);

                continue;
            }

            /* Decode index and value - make sure that parsing is not messed by local operating system regional settings. */
            int index;
            double value = 0;

            try
            {
                index = int.Parse(match.Groups[1].Value);

                if (index != 27)
                    value = double.Parse(match.Groups[2].Value);
            }
            catch (FormatException)
            {
                /* Report bad number and ignore reply. */
                _logger.LogWarning("invalid number in reply {Reply}", reply);

                continue;
            }

            if (index < 0)
            {
                /* Report bad number and ignore reply. */
                _logger.LogWarning("bad reply {Reply}", reply);

                continue;
            }

            /* Copy value to the appropriate field. */
            switch (index)
            {
                case 0:
                    response.Phases[0].Voltage.AcComponent!.Rms = value;
                    break;
                case 1:
                    response.Phases[1].Voltage.AcComponent!.Rms = value;
                    break;
                case 2:
                    response.Phases[2].Voltage.AcComponent!.Rms = value;
                    break;
                case 3:
                    response.Phases[0].Current.AcComponent!.Rms = value;
                    break;
                case 4:
                    response.Phases[1].Current.AcComponent!.Rms = value;
                    break;
                case 5:
                    response.Phases[2].Current.AcComponent!.Rms = value;
                    break;
                case 6:
                    response.Phases[0].Voltage.AcComponent!.Angle = value;
                    break;
                case 7:
                    response.Phases[1].Voltage.AcComponent!.Angle = value;
                    break;
                case 8:
                    response.Phases[2].Voltage.AcComponent!.Angle = value;
                    break;
                case 9:
                    response.Phases[0].Current.AcComponent!.Angle = value;
                    break;
                case 10:
                    response.Phases[1].Current.AcComponent!.Angle = value;
                    break;
                case 11:
                    response.Phases[2].Current.AcComponent!.Angle = value;
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
