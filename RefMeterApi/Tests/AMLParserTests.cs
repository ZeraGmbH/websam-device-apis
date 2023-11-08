using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RefMeterApi.Actions.Device;
using RefMeterApi.Models;
using RefMeterApiTests.PortMocks;
using SerialPortProxy;

namespace RefMeterApiTests;

[TestFixture]
public class AMLParserTests
{
    private readonly NullLogger<SerialPortConnection> _portLogger = new();

    private readonly DeviceLogger _deviceLogger = new();

    [TestCase("2WA", MeasurementModes.TwoWireActivePower)]
    [TestCase("3WA", MeasurementModes.ThreeWireActivePower)]
    [TestCase("4WA", MeasurementModes.FourWireActivePower)]
    public async Task Can_Process_Supported_Modes(string modeAsString, MeasurementModes mode)
    {
        var device = new SerialPortRefMeterDevice(
            SerialPortConnection.FromPortInstance(new FixedReplyMock(new[] { $"01;{modeAsString};xxx", "AMLACK" }), _portLogger),
            new NullLogger<SerialPortRefMeterDevice>()
        );

        var modes = await device.GetMeasurementModes();

        Assert.That(modes.Length, Is.EqualTo(1));
        Assert.That(modes[0], Is.EqualTo(mode));
    }

    [TestCase("01;2WA")]
    [TestCase("xxx;2WA;yyy")]
    [TestCase("")]
    [TestCase("01;XXX;ZZZ")]
    public async Task Will_Discard_Bad_Input(string reply)
    {
        var device = new SerialPortRefMeterDevice(
            SerialPortConnection.FromPortInstance(new FixedReplyMock(new[] {
                "01;2WA;2WAde",
                reply,
                "03;3WA;3WAde",
                "AMLACK"
            }), _portLogger),
            new NullLogger<SerialPortRefMeterDevice>()
        );

        var modes = await device.GetMeasurementModes();

        Assert.That(modes.Length, Is.EqualTo(2));
    }

    [TestCase("M=2WA", MeasurementModes.TwoWireActivePower)]
    [TestCase("M=3WA", MeasurementModes.ThreeWireActivePower)]
    [TestCase("M=4WA", MeasurementModes.FourWireActivePower)]
    public async Task Can_Detect_Active_Mode(string modeAsString, MeasurementModes mode)
    {
        var device = new SerialPortRefMeterDevice(
            SerialPortConnection.FromPortInstance(new FixedReplyMock(new[] { modeAsString, "ASTACK" }), _portLogger),
            new NullLogger<SerialPortRefMeterDevice>()
        );

        var reply = await device.GetActualMeasurementMode();

        Assert.That(reply, Is.EqualTo(mode));
    }

    [TestCase("XX=12")]
    [TestCase("M=")]
    [TestCase("M=JOJO")]
    public async Task Will_Detect_Unsupported_Mode(string reply)
    {
        var device = new SerialPortRefMeterDevice(
            SerialPortConnection.FromPortInstance(new FixedReplyMock(new[] {
                "A=1",
                reply,
                "B=2",
                "ASTACK"
            }), _portLogger),
            new NullLogger<SerialPortRefMeterDevice>()
        );

        var mode = await device.GetActualMeasurementMode();

        Assert.That(mode, Is.Null);
    }
}
