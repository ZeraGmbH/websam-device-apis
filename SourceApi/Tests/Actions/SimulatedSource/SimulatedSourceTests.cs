using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ZERA.WebSam.Shared.Actions;
using SourceApi.Actions.Source;
using SourceApi.Model;
using SourceApi.Tests.Actions.LoadpointValidator;

namespace SourceApi.Tests.Actions.Source;
internal class SimulatedSourceTests
{
    Mock<ILogger<SimulatedSource>> logger = new();
    SimulatedSource mock;

    [SetUp]
    public void SetUp()
    {
        mock = new(logger.Object, new SourceCapabilityValidator());
    }

    const string CONFIG_KEY_NUMBER_OF_PHASES = "SourceProperties:NumberOfPhases";

    #region PositiveTestCases
    [Test]
    [TestCaseSource(typeof(LoadpointValidatorTestData), nameof(LoadpointValidatorTestData.ValidLoadpoints))]
    public async Task TestValidTurnOn_Async(TargetLoadpoint loadpoint)
    {
        // Arrange
        var capabilities = SimulatedSourceTestData.GetSourceCapabilitiesForNumberOfPhases(loadpoint.Phases.Count);

        ISource source = GenerateSimulatedSource(capabilities: capabilities);

        // Act
        var result = await source.SetLoadpointAsync(new NoopInterfaceLogger(), loadpoint);

        // Assert
        var currentLoadpoint = await source.GetCurrentLoadpointAsync(new NoopInterfaceLogger());

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.SUCCESS));
        Assert.That(loadpoint, Is.EqualTo(currentLoadpoint));

        var info = await source.GetActiveLoadpointInfoAsync(new NoopInterfaceLogger());
        Assert.That(info.IsActive, Is.EqualTo(true));
    }

    [Test]
    [TestCaseSource(typeof(LoadpointValidatorTestData), nameof(LoadpointValidatorTestData.ValidLoadpoints))]
    public async Task TestValidTurnOff_Async(TargetLoadpoint loadpoint)
    {
        // Arrange
        var capabilities = SimulatedSourceTestData.GetSourceCapabilitiesForNumberOfPhases(loadpoint.Phases.Count);

        ISource source = GenerateSimulatedSource(capabilities: capabilities);

        await source.SetLoadpointAsync(new NoopInterfaceLogger(), loadpoint);

        // Act
        var result = await source.TurnOffAsync(new NoopInterfaceLogger());

        // Assert
        await source.GetCurrentLoadpointAsync(new NoopInterfaceLogger());

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.SUCCESS));

        var info = await source.GetActiveLoadpointInfoAsync(new NoopInterfaceLogger());
        Assert.That(info.IsActive, Is.EqualTo(false));
    }
    #endregion

    #region LoadpointIssues
    [Test]
    [TestCaseSource(typeof(SimulatedSourceTestData), nameof(SimulatedSourceTestData.ValidLoadpointsWithOneOrThreePhases))]
    public async Task TestTurnOnWithInvalidLoadpoint_Async(TargetLoadpoint loadpoint)
    {
        // Arrange
        var capabilities = SimulatedSourceTestData.DefaultTwoPhaseSourceCapabilities;

        ISource source = GenerateSimulatedSource(capabilities: capabilities);

        // Act
        var result = await source.SetLoadpointAsync(new NoopInterfaceLogger(), loadpoint);

        // Assert
        var currentLoadpoint = await source.GetCurrentLoadpointAsync(new NoopInterfaceLogger());

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.LOADPOINT_NOT_SUITABLE_DIFFERENT_NUMBER_OF_PHASES));
        Assert.That(currentLoadpoint, Is.Null);


        var info = await source.GetActiveLoadpointInfoAsync(new NoopInterfaceLogger());
        Assert.That(info.IsActive, Is.EqualTo(null));
    }
    #endregion

    #region CapabilityIssues
    [Test]
    public async Task TestTooHighVoltage_Async()
    {
        // Arrange 
        ISource source = GenerateSimulatedSource();
        TargetLoadpoint lp = LoadpointValidatorTestData.Loadpoint001_3AC_valid;
        lp.Phases[0].Voltage.AcComponent!.Rms = new(500);

        // Act
        var result = await source.SetLoadpointAsync(new NoopInterfaceLogger(), lp);

        // Assert
        var currentLoadpoint = await source.GetCurrentLoadpointAsync(new NoopInterfaceLogger());

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.LOADPOINT_NOT_SUITABLE_VOLTAGE_INVALID));
        Assert.That(currentLoadpoint, Is.Null);
    }

    [Test]
    public async Task TestTooHighCurrent_Async()
    {
        // Arrange 
        ISource source = GenerateSimulatedSource();
        TargetLoadpoint lp = LoadpointValidatorTestData.Loadpoint001_3AC_valid;
        lp.Phases[0].Current.AcComponent!.Rms = new(100);

        // Act
        var result = await source.SetLoadpointAsync(new NoopInterfaceLogger(), lp);

        // Assert
        var currentLoadpoint = await source.GetCurrentLoadpointAsync(new NoopInterfaceLogger());

        Assert.That(result, Is.EqualTo(SourceApiErrorCodes.LOADPOINT_NOT_SUITABLE_CURRENT_INVALID));
        Assert.That(currentLoadpoint, Is.Null);
    }
    #endregion

    #region HelperFunctions
    private static ISource GenerateSimulatedSource(Mock<ILogger<SimulatedSource>>? loggerMock = null, Mock<IConfiguration>? configMock = null, SourceCapabilities? capabilities = null)
    {
        loggerMock ??= new();

        if (capabilities == null)
            return new SimulatedSource(loggerMock.Object, new SourceCapabilityValidator());

        return new SimulatedSource(loggerMock.Object, capabilities, new SourceCapabilityValidator());
    }
    #endregion

    [Test]
    public async Task Returns_Correct_Dosage_Progress_Async()
    {
        await mock.SetLoadpointAsync(new NoopInterfaceLogger(), GetLoadpoint());
        await mock.SetDosageEnergyAsync(new NoopInterfaceLogger(), new(20), new(1));
        await mock.StartDosageAsync(new NoopInterfaceLogger());

        var result = await mock.GetDosageProgressAsync(new NoopInterfaceLogger(), new(1));

        Assert.That(result.Active, Is.EqualTo(true));
        Assert.That((double)result.Remaining, Is.LessThan(20));
        Assert.That((double)result.Progress, Is.GreaterThan(0));
    }

    [Test]
    public async Task Turns_Off_Loadpoint_Async()
    {
        var loadpoint = GetLoadpoint();
        await mock.SetLoadpointAsync(new NoopInterfaceLogger(), loadpoint);

        await mock.TurnOffAsync(new NoopInterfaceLogger());

        foreach (var phase in loadpoint.Phases)
        {
            Assert.That(phase.Voltage.On, Is.EqualTo(true));
            Assert.That(phase.Current.On, Is.EqualTo(true));
        }

        var info = await mock.GetActiveLoadpointInfoAsync(new NoopInterfaceLogger());

        Assert.That(info.IsActive, Is.EqualTo(false));
    }

    private static TargetLoadpoint GetLoadpoint()
    {
        return new()
        {
            Frequency = new() { Value = new(50) },
            Phases = new List<TargetLoadpointPhase>(){
                new TargetLoadpointPhase(){
                    Current = new(){AcComponent = new() {Angle=new(0), Rms=new(10)}, On=true},
                    Voltage = new(){AcComponent = new() {Angle=new(0), Rms=new(220)}, On=true}
                },
                new TargetLoadpointPhase(){
                    Current = new(){AcComponent = new() {Angle=new(120), Rms=new(10)}, On=true},
                    Voltage = new(){AcComponent = new() {Angle=new(120), Rms=new(220)}, On=true}
                },
                new TargetLoadpointPhase(){
                    Current = new(){AcComponent = new() {Angle=new(240), Rms=new(10)}, On=true},
                    Voltage = new(){AcComponent = new() {Angle=new(240), Rms=new(220)}, On=true}
                }
            }
        };
    }
}

