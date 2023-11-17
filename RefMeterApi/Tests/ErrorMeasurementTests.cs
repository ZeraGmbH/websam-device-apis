using Microsoft.Extensions.Logging.Abstractions;
using RefMeterApi.Actions.Device;
using RefMeterApi.Models;
using SerialPortProxy;

namespace RefMeterApiTests;

[TestFixture]
public class ErrorMeasurementTests
{
    class PortMock : ISerialPort
    {
        private readonly Queue<string> _queue = new();

        public string[] StatusResponse = { };

        public string ActiveParameters = "";

        public void Dispose()
        {
        }

        public string ReadLine()
        {
            if (_queue.TryDequeue(out var reply))
                return reply;

            throw new TimeoutException("queue is empty");
        }

        public void WriteLine(string command)
        {
            switch (command)
            {
                case "AEB0":
                case "AEB1":
                    _queue.Enqueue("AEBACK");
                    break;
                case "AEE":
                    _queue.Enqueue("AEEACK");
                    break;
                case "AES1":
                    Array.ForEach(StatusResponse, r => _queue.Enqueue(r));

                    _queue.Enqueue("AESACK");
                    break;
                default:
                    if (command.StartsWith("AEP"))
                    {
                        ActiveParameters = command[3..];

                        _queue.Enqueue("AEPACK");
                    }

                    break;
            }
        }

    }

    private readonly NullLogger<SerialPortRefMeterDevice> _logger = new();

    private readonly PortMock _port = new();

    private SerialPortConnection Device = null!;

    [SetUp]
    public void Setup()
    {
        Device = SerialPortConnection.FromPortInstance(_port, new NullLogger<SerialPortConnection>());
    }

    [Test]
    public async Task Can_Start_Error_Measure()
    {
        var cut = new SerialPortRefMeterDevice(Device, _logger);

        await cut.StartErrorMeasurement(false);
        await cut.StartErrorMeasurement(true);
    }

    [Test]
    public async Task Can_Abort_Error_Measure()
    {
        var cut = new SerialPortRefMeterDevice(Device, _logger);

        await cut.AbortErrorMeasurement();
    }

    [Test]
    public async Task Can_Request_Error_Management_Status()
    {
        var cut = new SerialPortRefMeterDevice(Device, _logger);

        var status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.NotActive));
            Assert.That(status.Energy, Is.Null);
            Assert.That(status.Progress, Is.Null);
            Assert.That(status.ErrorValue, Is.Null);
        });

        _port.StatusResponse = new string[] { "00", "oo.oo", "0.000000;0.000000" };

        status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.NotActive));
            Assert.That(status.ErrorValue, Is.Null);
            Assert.That(status.Progress, Is.EqualTo(0d));
            Assert.That(status.Energy, Is.EqualTo(0d));
        });

        _port.StatusResponse = new string[] { "11", "--.--", "0.000000;0.000000" };

        status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.Active));
            Assert.That(status.ErrorValue, Is.Null);
            Assert.That(status.Progress, Is.EqualTo(0d));
            Assert.That(status.Energy, Is.EqualTo(0d));
        });

        _port.StatusResponse = new string[] { "12", "--.--", "51.234112;912.38433" };

        status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.Running));
            Assert.That(status.ErrorValue, Is.Null);
            Assert.That(status.Progress, Is.EqualTo(51.234112d));
            Assert.That(status.Energy, Is.EqualTo(912.38433d));
        });

        _port.StatusResponse = new string[] { "13", "0.5", "51.234112;912.38433" };

        status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.Finished));
            Assert.That(status.ErrorValue, Is.EqualTo(0.5d));
            Assert.That(status.Progress, Is.EqualTo(51.234112d));
            Assert.That(status.Energy, Is.EqualTo(912.38433d));
        });

        _port.StatusResponse = new string[] { "13", "-0.2", "51.234112;912.38433" };

        status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.Finished));
            Assert.That(status.ErrorValue, Is.EqualTo(-0.2d));
            Assert.That(status.Progress, Is.EqualTo(51.234112d));
            Assert.That(status.Energy, Is.EqualTo(912.38433d));
        });

        _port.StatusResponse = new string[] { "03", "-.2", "51.234112;912.38433" };

        status = await cut.GetErrorStatus();

        Assert.That(status, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(status.State, Is.EqualTo(ErrorMeasurementStates.Finished));
            Assert.That(status.ErrorValue, Is.EqualTo(-0.2d));
            Assert.That(status.Progress, Is.EqualTo(51.234112d));
            Assert.That(status.Energy, Is.EqualTo(912.38433d));
        });
    }

    [TestCase(10000, 50000, "100000;4;50000;0")]
    [TestCase(100000, 2500050, "100000;5;250005;1")]
    [TestCase(100000, 2500055, "100000;5;250006;1")]
    [TestCase(100000, 99999949, "100000;5;999999;2")]
    [TestCase(0.1, 99999949, "10000;0;999999;2")]
    [TestCase(0.001235, 99999949, "124;0;999999;2")]
    [TestCase(10000.3, 50000, "100003;4;50000;0")]
    [TestCase(10000.5, 50000, "100005;4;50000;0")]
    [TestCase(100000.3, 2500050, "100000;5;250005;1")]
    [TestCase(100000.5, 2500050, "100000;5;250005;1")]
    [TestCase(100000.51, 2500050, "100001;5;250005;1")]
    public async Task Can_Set_Error_Measurement_Parameters(double meterConstant, long impluses, string expected)
    {
        var cut = new SerialPortRefMeterDevice(Device, _logger);

        await cut.SetErrorMeasurementParameters(meterConstant, impluses);

        Assert.That(_port.ActiveParameters, Is.EqualTo(expected));
    }

    [TestCase(1, "1", 0)]
    [TestCase(10, "10", 0)]
    [TestCase(100, "100", 0)]
    [TestCase(1000, "1000", 0)]
    [TestCase(10000, "10000", 0)]
    [TestCase(100000, "100000", 0)]
    [TestCase(1000000, "100000", 1)]
    [TestCase(10000000, "100000", 2)]
    [TestCase(100000000, "100000", 3)]
    [TestCase(1000000000, "100000", 4)]
    [TestCase(10000000000, "100000", 5)]
    [TestCase(99999944, "999999", 2)]
    [TestCase(99999950, "100000", 3)]
    [TestCase(99999999, "100000", 3)]
    public void Can_Clip_Numbers_To_Protocol_Restriction(long number, string expectedString, int expectedScale)
    {
        var (asString, power) = SerialPortRefMeterDevice.ClipNumberToProtocol(number, 11);
        Assert.Multiple(() =>
        {
            Assert.That(asString, Is.EqualTo(expectedString));
            Assert.That(power, Is.EqualTo(expectedScale));
        });
    }
}