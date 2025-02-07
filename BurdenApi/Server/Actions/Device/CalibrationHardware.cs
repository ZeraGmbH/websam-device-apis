using BurdenApi.Models;
using ZERA.WebSam.Shared.Models;
using ZERA.WebSam.Shared.Models.Source;
using ZERA.WebSam.Shared.DomainSpecific;
using ZERA.WebSam.Shared.Models.Logging;
using ZERA.WebSam.Shared.Models.ReferenceMeter;
using ZERA.WebSam.Shared.Provider;
using ZERA.WebSam.Shared.Provider.Exceptions;

namespace BurdenApi.Actions.Device;

/// <summary>
/// Implementation of a calibration environment.
/// </summary>
public class CalibrationHardware(ISource source, ISourceHealthUtils sourceHealth, IRefMeter refMeter, IBurden burden, IInterfaceLogger logger) : ICalibrationHardware
{
    /// <inheritdoc/>
    public IBurden Burden { get; } = burden;

    private Voltage _VoltageRange;

    private Current _CurrentRange;

    /// <inheritdoc/>
    public async Task<RefMeterValueWithQuantity> MeasureAsync(Calibration calibration, bool voltageNotCurrent)
    {
        // Apply the calibration to the burden.        
        await Burden.SetTransientCalibrationAsync(calibration, logger);

        // Check for simulation mode to speed up tests.
        var isMock = Burden is IBurdenMock mockedBurden && mockedBurden.HasMockedSource;

        // Relax a bit.
        if (!isMock) await Task.Delay(5000);

        for (var retry = 3; retry-- > 0;)
        {
            // Ask device for values.
            var values = await refMeter.GetActualValuesUncachedAsync(logger, -1, true);

            // Ask for current ranges.
            var status = await refMeter.GetRefMeterStatusAsync(logger);

            // Check for keeping the ranges.
            if ((double)_VoltageRange != 0 && (double)_CurrentRange != 0)
                if (status.VoltageRange != _VoltageRange || status.CurrentRange != _CurrentRange)
                {
                    if (status.VoltageRange != _VoltageRange)
                        await refMeter.SetVoltageRangeAsync(logger, _VoltageRange);

                    if (status.CurrentRange != _CurrentRange)
                        await refMeter.SetCurrentRangeAsync(logger, _CurrentRange);

                    continue;
                }

            // Report.
            var phase = values.Phases[0];
            var apparentPower = phase.ApparentPower;
            var factor = phase.PowerFactor;

            if (apparentPower == null || factor == null)
                throw new InvalidOperationException("insufficient actual values");

            return new()
            {
                ApparentPower = apparentPower.Value,
                CurrentRange = status.CurrentRange,
                Frequency = values.Frequency,
                Phase = phase,
                PowerFactor = factor.Value,
                Rms = voltageNotCurrent ? (double?)phase.Voltage.AcComponent?.Rms : (double?)phase.Current.AcComponent?.Rms,
                VoltageRange = status.VoltageRange,
            };
        }

        throw new InvalidOperationException("reference meter keeps changing ranges - unable to get reliable actual values");
    }


    /// <inheritdoc/>
    public async Task<PrepareResult> PrepareAsync(bool voltageNotCurrent, string range, double percentage, Frequency frequency, bool detectRange, ApparentPower power, bool fixedPercentage = true)
    {
        // Get the capabilities from the source.
        var caps = await source.GetCapabilitiesAsync(logger);

        var minRange = voltageNotCurrent ? (double?)caps.Phases[0].AcVoltage?.Min : (double?)caps.Phases[0].AcCurrent?.Min;
        var maxRange = voltageNotCurrent ? (double?)caps.Phases[0].AcVoltage?.Max : (double?)caps.Phases[0].AcCurrent?.Max;

        // Analyse the range pattern.
        var rawValue = BurdenUtils.ParseRange(range);
        var rangeValue = percentage * rawValue;

        if (!fixedPercentage && minRange.HasValue && maxRange.HasValue)
            if (rangeValue < minRange.Value)
            {
                rangeValue = minRange.Value;
                percentage = rangeValue / rawValue;
            }
            else if (rangeValue > maxRange.Value)
            {
                rangeValue = maxRange.Value;
                percentage = rangeValue / rawValue;
            }

        // Get the scaling factors and use the best fit value in the allowed precision range.
        var stepSize = caps.Phases.Count < 1
            ? null
            : voltageNotCurrent
            ? (double?)caps.Phases[0].AcVoltage?.PrecisionStepSize
            : (double?)caps.Phases[0].AcCurrent?.PrecisionStepSize;

        var stepFactor = stepSize ?? 0;

        if (stepFactor > 0)
            rangeValue = stepFactor * Math.Round(rangeValue / stepFactor);

        // Create the IEC loadpoint.
        var lp = new TargetLoadpoint
        {
            Frequency = { Mode = FrequencyMode.SYNTHETIC, Value = frequency },
            Phases = {
                new() {
                    Current = new() { On = !voltageNotCurrent, AcComponent = new() { Rms = new(voltageNotCurrent ? 0 : rangeValue)}  } ,
                    Voltage = new() { On = voltageNotCurrent, AcComponent = new() { Rms = new(voltageNotCurrent ? rangeValue: 0)} }
                },
                new() { Current = new() { On = false, AcComponent = new() }, Voltage = new() { On = false, AcComponent = new() } },
                new() { Current = new() { On = false, AcComponent = new() }, Voltage = new() { On = false, AcComponent = new() } },
        }
        };

        // Set the loadpioint.
        var status = await sourceHealth.SetLoadpointAsync(logger, lp);

        if (status != SourceApiErrorCodes.SUCCESS) throw new InvalidOperationException($"bad loadpoint: {status}");

        // Get the best fit on reference meter range.
        if (detectRange)
        {
            // Use manual.
            await refMeter.SetAutomaticAsync(logger, false, false, false);

            // Get all the supported ranges.
            var currentRanges = await refMeter.GetCurrentRangesAsync(logger);
            var voltageRanges = await refMeter.GetVoltageRangesAsync(logger);

            if (currentRanges.Length < 1) throw new InvalidOperationException("no reference meter current ranges found");
            if (voltageRanges.Length < 1) throw new InvalidOperationException("no reference meter voltage ranges found");

            Array.Sort(currentRanges);
            Array.Sort(voltageRanges);

            // Calculate the current from the apparent power and the voltage.
            var otherRange = (double)(power * percentage * percentage) / rangeValue;
            var voltageRange = voltageNotCurrent ? rangeValue : otherRange;
            var currentRange = voltageNotCurrent ? otherRange : rangeValue;

            // Find the first bigger ones.
            var currentIndex = Array.FindIndex(currentRanges, r => (double)r >= currentRange);
            var voltageIndex = Array.FindIndex(voltageRanges, r => (double)r >= voltageRange);

            // Use the ranges.            
            _VoltageRange = voltageIndex < 0 ? voltageRanges[^1] : voltageRanges[voltageIndex];
            _CurrentRange = currentIndex < 0 ? currentRanges[^1] : currentRanges[currentIndex];

            await refMeter.SetVoltageRangeAsync(logger, _VoltageRange);
            await refMeter.SetCurrentRangeAsync(logger, _CurrentRange);
        }
        else
        {
            // Use automatic.
            _VoltageRange = Voltage.Zero;
            _CurrentRange = Current.Zero;

            await refMeter.SetAutomaticAsync(logger, true, true, false);
        }

        // Choose the PLL channel - put in manual mode before.
        await refMeter.SelectPllChannelAsync(logger, voltageNotCurrent ? PllChannel.U1 : PllChannel.I1);

        // Configure reference meter.
        await refMeter.SetActualMeasurementModeAsync(logger, MeasurementModes.MqBase);

        // Check for simulation mode to speed up tests.
        var isMock = Burden is IBurdenMock mockedBurden && mockedBurden.HasMockedSource;

        // Wait a bit for stabilisation.
        if (!isMock) await Task.Delay(10000);

        // Report new pecentage
        return new()
        {
            CurrentRange = _CurrentRange,
            Factor = percentage,
            IsVoltageNotCurrentBurden = voltageNotCurrent,
            Range = rangeValue,
            VoltageRange = _VoltageRange,
        };
    }

    /// <inheritdoc/>
    public async Task<GoalValueWithQuantity> MeasureBurdenAsync(bool voltageNotCurrent)
    {
        var values = await Burden.MeasureAsync(logger);

        return new()
        {
            ApparentPower = values.ApparentPower,
            PowerFactor = values.PowerFactor,
            Rms = voltageNotCurrent ? (double?)values.Voltage : (double?)values.Current,
        };
    }
}