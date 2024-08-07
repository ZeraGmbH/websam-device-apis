
using Microsoft.Extensions.Logging.Abstractions;

using SerialPortProxy;
using ZERA.WebSam.Shared.Actions;
using SourceApi.Actions.SerialPort;
using SourceApi.Actions.SerialPort.FG30x;
using SourceApi.Actions.Source;
using SourceApi.Tests.Actions.Dosage.PortMocks;

namespace SourceApi.Tests.Actions.Dosage;

[TestFixture]
public class DosageFGTests
{
    static DosageFGTests()
    {
        SerialPortConnection.ActivateUnitTestMode(30000);
    }

    private readonly NullLogger<ISerialPortConnection> _portLogger = new();

    private readonly DeviceLogger<SerialPortFGSource> _deviceLogger = new();

    private ISource CreateDevice(params string[] replies)
    {
        var device = new SerialPortFGSource(_deviceLogger, SerialPortConnection.FromMockedPortInstance(new FixedReplyMock(replies), _portLogger), new CapabilitiesMap(), new SourceCapabilityValidator());

        device.SetAmplifiers(new NoopInterfaceLogger(), Model.VoltageAmplifiers.V210, Model.CurrentAmplifiers.V200, Model.VoltageAuxiliaries.V210, Model.CurrentAuxiliaries.V200);

        return device;
    }

    [Test]
    public async Task Can_Turn_Off_DOS_Mode()
    {
        await CreateDevice(["OK3CM4"]).SetDosageMode(new NoopInterfaceLogger(), false);
    }

    [Test]
    public async Task Can_Turn_On_DOS_Mode()
    {
        await CreateDevice(["OK3CM3"]).SetDosageMode(new NoopInterfaceLogger(), true);
    }

    [Test]
    public async Task Can_Start_Dosage()
    {
        await CreateDevice(["OK3CM1"]).StartDosage(new NoopInterfaceLogger());
    }

    [Test]
    public async Task Can_Abort_Dosage()
    {
        await CreateDevice(["OK3CM2"]).CancelDosage(new NoopInterfaceLogger());
    }

    [TestCase(2, "113834")]
    [TestCase(1, "113834")]
    [TestCase(0, "113834")]
    [TestCase(2, "0")]
    [TestCase(2, "330E-1")]
    [TestCase(2, "333E2")]
    public async Task Can_Read_Dosage_Progress(int dosage, string remaining)
    {
        /* Warning: knows about internal sequence of requests. */
        var progress = await CreateDevice([
            $"OK3SA1;{dosage}",
            $"OK3MA1;{remaining}",
            "OK3PA45;918.375",
        ]).GetDosageProgress(new NoopInterfaceLogger(), new(1d));

        var rest = double.Parse(remaining) * 1000d;

        Assert.Multiple(() =>
        {
            Assert.That(progress.Active, Is.EqualTo(dosage == 2));
            Assert.That((double)progress.Remaining, Is.EqualTo(rest));
            Assert.That((double)progress.Progress, Is.EqualTo(918375 - rest));
            Assert.That((double)progress.Total, Is.EqualTo(918375));
        });
    }

    [TestCase(1)]
    [TestCase(1.23)]
    [TestCase(1E5)]
    [TestCase(1E-5)]
    [TestCase(3)]
    public async Task Can_Set_Impules_From_Energy(double energy)
    {
        var mock = new CommandPeekMock(["OK3PS45"]);

        var device = new SerialPortFGSource(_deviceLogger, SerialPortConnection.FromMockedPortInstance(mock, _portLogger), new CapabilitiesMap(), new SourceCapabilityValidator());

        device.SetAmplifiers(new NoopInterfaceLogger(), Model.VoltageAmplifiers.V210, Model.CurrentAmplifiers.V200, Model.VoltageAuxiliaries.V210, Model.CurrentAuxiliaries.V200);

        await device.SetDosageEnergy(new NoopInterfaceLogger(), new(energy), new(1d));

        Assert.That(mock.Commands[0], Is.EqualTo($"3PS45;{energy / 1000d}"));
    }
}
