using System.Text.RegularExpressions;
using BurdenApi.Models;
using NUnit.Framework;
using RefMeterApi.Actions.Device;
using RefMeterApi.Models;
using SourceApi.Actions.Source;
using SourceApi.Model;
using ZERA.WebSam.Shared.DomainSpecific;
using ZERA.WebSam.Shared.Models.Logging;

namespace BurdenApi.Actions.Device;

/// <summary>
/// Implementation of a calibration environment.
/// </summary>
public class CalibrationHardware(ISource source, IRefMeter refMeter, IBurden burden, IInterfaceLogger logger) : ICalibrationHardware
{
    private static readonly Regex _RangePattern = new("^([^/]+)(/(3|v3))?$");

    /// <inheritdoc/>
    public IBurden Burden { get; } = burden;

    /// <inheritdoc/>
    public IRefMeter ReferenceMeter { get; } = refMeter;

    /// <inheritdoc/>
    public Task<GoalValue> MeasureAsync(Calibration calibration)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task SetLoadpointAsync(string range, double percentage, Frequency frequency, bool detectRange, GoalValue goal)
    {
        // Analyse the range pattern - assume some number optional followed by a scaling.
        var match = _RangePattern.Match(range);

        if (!match.Success) throw new ArgumentException(range, nameof(range));

        var rangeValue = percentage * double.Parse(match.Groups[1].Value);

        switch (match.Groups[3].Value)
        {
            case "3":
                rangeValue /= 3;
                break;
            case "v3":
                rangeValue /= Math.Sqrt(3);
                break;
        }

        // Change power factor to angle.
        var angle = (Angle)goal.PowerFactor;

        // Check the type of burden.
        var burdenInfo = await Burden.GetVersionAsync(logger);
        var isVoltageNotCurrent = burdenInfo.IsVoltageNotCurrent;

        // Create the IEC loadpoint.
        var lp = new TargetLoadpoint
        {
            Frequency = { Mode = FrequencyMode.SYNTHETIC, Value = frequency },
            Phases = {
                new() {
                    Current = new() {
                        On = !isVoltageNotCurrent,
                        AcComponent = new() { Rms = new(isVoltageNotCurrent ? 0 : rangeValue), Angle = Angle.Zero }
                    },
                    Voltage = new() {
                        On = isVoltageNotCurrent,
                        AcComponent =  new() { Rms = new(isVoltageNotCurrent ? rangeValue : 0), Angle = (new Angle(360) - angle).Normalize() }
                    }
                },
                new() { Current = new() { On = false }, Voltage = new() { On = false } },
                new() { Current = new() { On = false }, Voltage = new() { On = false } },
            }
        };

        // Set the loadpioint.
        var status = await source.SetLoadpointAsync(logger, lp);

        if (status != SourceApiErrorCodes.SUCCESS) throw new InvalidOperationException("bad loadpoint");

        // Get the best fit on reference meter range.
        if (detectRange)
        {
            // Use manual.
            await ReferenceMeter.SetAutomaticAsync(logger, false, false, false);

            // Get all the supported ranges.
            var currentRanges = await ReferenceMeter.GetCurrentRangesAsync(logger);
            var voltageRanges = await ReferenceMeter.GetVoltageRangesAsync(logger);

            if (currentRanges.Length < 1) throw new InvalidOperationException("no reference meter current ranges found");
            if (voltageRanges.Length < 1) throw new InvalidOperationException("no reference meter voltage ranges found");

            Array.Sort(currentRanges);
            Array.Sort(voltageRanges);

            // Calculate the values used.
            var otherValue = goal.ApparentPower / rangeValue;
            var testVoltage = isVoltageNotCurrent ? rangeValue : (double)otherValue;
            var testCurrent = isVoltageNotCurrent ? (double)otherValue : rangeValue;

            // Find the first bigger ones.
            var currentIndex = Array.FindIndex(currentRanges, r => (double)r >= testCurrent);
            var voltageIndex = Array.FindIndex(voltageRanges, r => (double)r >= testVoltage);

            // Use the ranges.            
            await ReferenceMeter.SetVoltageRangeAsync(logger, voltageIndex < 0 ? voltageRanges[^1] : voltageRanges[voltageIndex]);
            await ReferenceMeter.SetCurrentRangeAsync(logger, currentIndex < 0 ? currentRanges[^1] : currentRanges[currentIndex]);
        }
        else
        {
            // Use automatic.
            await ReferenceMeter.SetAutomaticAsync(logger, true, true, false);
        }

        // Choose the PLL channel - put in manual mode before.
        await ReferenceMeter.SelectPllChannelAsync(logger, isVoltageNotCurrent ? PllChannel.U1 : PllChannel.I1);
    }
}