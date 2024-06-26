using Microsoft.Extensions.DependencyInjection;
using RefMeterApi.Models;
using SharedLibrary;
using SharedLibrary.Models.Logging;
using SourceApi.Actions.Source;
using SourceApi.Model;

namespace RefMeterApi.Actions.Device;

/// <summary>
/// 
/// </summary>
public interface IDCRefMeterMock : IMockRefMeter
{

}

/// <summary>
/// 
/// </summary>
public class DCRefMeterMock : RefMeterMock, IDCRefMeterMock
{
    private readonly IServiceProvider _di;

    /// <summary>
    /// 
    /// </summary>
    public DCRefMeterMock(IServiceProvider di)
    {
        _di = di;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lp"></param>
    /// <returns></returns>
    public override MeasuredLoadpoint CalcMeasureOutput(TargetLoadpoint lp)
    {
        var measureOutputPhases = new List<MeasuredLoadpointPhase>();

        var current = lp.Phases[0].Current.On ? lp.Phases[0].Current.DcComponent : 0;
        var voltage = lp.Phases[0].Voltage.On ? lp.Phases[0].Voltage.DcComponent : 0;

        var activePower = current * voltage;

        var measureOutputPhase = new MeasuredLoadpointPhase()
        {
            Current = new()
            {
                DcComponent = current
            },
            Voltage = new()
            {
                DcComponent = voltage
            },
            ActivePower = activePower
        };

        measureOutputPhases.Add(measureOutputPhase);

        return new()
        {
            Phases = measureOutputPhases,
            ActivePower = activePower
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="firstActiveVoltagePhase">Not relevant in dc</param>
    /// <returns></returns>
    public async override Task<MeasuredLoadpoint> GetActualValues(IInterfaceLogger logger, int firstActiveVoltagePhase = -1)
    {
        var loadpoint = await GetLoadpoint(logger);

        var mo = CalcMeasureOutput(loadpoint);

        CalculateDeviations(mo);

        return mo;
    }

    private async Task<TargetLoadpoint> GetLoadpoint(IInterfaceLogger logger)
    {
        var r = Random.Shared;

        var source = _di.GetRequiredService<ISource>();

        var loadpoint = source.GetCurrentLoadpoint(logger) ?? new TargetLoadpoint()
        {
            Phases = [
                new () {
                    Current = new() { DcComponent = 0},
                    Voltage = new() { DcComponent = 0},
                },
            ]
        };

        loadpoint = LibUtils.DeepCopy(loadpoint);

        var info = source.GetActiveLoadpointInfo(logger);
        var currentSwitchedOffForDosage = await source.CurrentSwitchedOffForDosage(logger);

        var phase = loadpoint.Phases[0];
        if (phase.Voltage.On && info.IsActive == false) phase.Voltage.On = false;
        if (phase.Current.On && (info.IsActive == false || currentSwitchedOffForDosage)) phase.Current.On = false;

        return loadpoint;
    }

    private static void CalculateDeviations(MeasuredLoadpoint mo)
    {
        var voltage = mo.Phases[0].Voltage.DcComponent;
        var current = mo.Phases[0].Current.DcComponent;
        var activePower = mo.Phases[0].Voltage.DcComponent;

        if (voltage != null)
            mo.Phases[0].Voltage.DcComponent = Math.Abs(GetRandomNumberWithAbsoluteDeviation((double)voltage, 0.01));
        if (current != null)
            mo.Phases[0].Current.DcComponent = Math.Abs(GetRandomNumberWithAbsoluteDeviation((double)current, 0.01));
        if (activePower != null)
        {
            mo.Phases[0].ActivePower = Math.Abs(GetRandomNumberWithAbsoluteDeviation((double)activePower, 0.01));
            mo.ActivePower = mo.Phases[0].ActivePower;
        }
    }
}
