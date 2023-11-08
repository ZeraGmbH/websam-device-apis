using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using RefMeterApi.Actions.Device;
using RefMeterApiTests.PortMocks;
using SerialPortProxy;

namespace RefMeterApiTests;

[TestFixture]
public class DosageTests
{
    private readonly NullLogger<SerialPortConnection> _portLogger = new();

    private readonly DeviceLogger _deviceLogger = new();

    private IRefMeterDevice CreateDevice(params string[] replies) => new SerialPortRefMeterDevice(SerialPortConnection.FromPortInstance(new FixedReplyMock(replies), _portLogger), _deviceLogger);

    [Test]
    public async Task Can_Turn_Off_DOS_Mode()
    {
        await CreateDevice(new[] { "SOK3CM4" }).SetDosageMode(false);
    }

    [Test]
    public async Task Can_Turn_On_DOS_Mode()
    {
        await CreateDevice(new[] { "SOK3CM3" }).SetDosageMode(true);
    }

    [Test]
    public async Task Can_Start_Dosage()
    {
        await CreateDevice(new[] { "SOK3CM1" }).StartDosage();
    }

    [Test]
    public async Task Can_Abort_Dosage()
    {
        await CreateDevice(new[] { "SOK3CM2" }).CancelDosage();
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
        var progress = await CreateDevice(new[] {
            $"SOK3SA1;{dosage}",
            $"SOK3MA4;{remaining}",
            "SOK3MA5;303541"
        }).GetDosageProgress();

        Assert.Multiple(() =>
        {
            Assert.That(progress.Active, Is.EqualTo(dosage == 2));
            Assert.That(progress.Remaining, Is.EqualTo(double.Parse(remaining, CultureInfo.InvariantCulture)));
            Assert.That(progress.Progress, Is.EqualTo(303541));
        });
    }

    [TestCase(1)]
    [TestCase(1.23)]
    [TestCase(1E5)]
    [TestCase(1E-5)]
    [TestCase(3)]
    public async Task Can_Set_Impules_From_Energy(double energy)
    {
        var mock = new CommandPeekMock(new[] {
            "UB=60",
            "IB=2",
            "M=4WA",
            "ASTACK",
            "SOK3PS46"
        });

        var device = new SerialPortRefMeterDevice(SerialPortConnection.FromPortInstance(mock, _portLogger), _deviceLogger);

        await device.SetDosageEnergy(energy);

        Assert.That(mock.Commands[1], Is.EqualTo($"S3PS46;{(long)(6000E5 * energy):0000000000}"));
    }
}