using Microsoft.Extensions.Logging.Abstractions;
using RefMeterApi.Actions.Device;
using RefMeterApiTests.PortMocks;
using SerialPortProxy;

namespace RefMeterApiTests;

[TestFixture]
public class AMEParserTests
{
    private readonly NullLogger<ISerialPortConnection> _portLogger = new();

    private readonly DeviceLogger _deviceLogger = new();

    private IRefMeter CreateDevice(params string[] replies) => new SerialPortMTRefMeter(SerialPortConnection.FromPortInstance(new FixedReplyMock(replies), _portLogger), _deviceLogger);

    [Test]
    public async Task Can_Parse_AME_Reply()
    {
        var ameResponse = new List<string>(File.ReadAllLines(@"TestData/ameReply.txt"));

        ameResponse.Insert(0, "ATIACK");

        var device = CreateDevice(ameResponse.ToArray());
        var parsed = await device.GetActualValues();

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Frequency, Is.EqualTo(50).Within(0.5));
            Assert.That(parsed.Phases[0].Voltage, Is.EqualTo(20).Within(0.5));
            Assert.That(parsed.Phases[1].Current, Is.EqualTo(0.1).Within(0.05));
            Assert.That(parsed.Phases[1].AngleVoltage, Is.EqualTo(120).Within(0.5));
            Assert.That(parsed.Phases[2].AngleCurrent, Is.EqualTo(240).Within(0.5));
            Assert.That(parsed.Phases[0].PowerFactor, Is.EqualTo(0.99965084));

            Assert.That(parsed.PhaseOrder, Is.EqualTo("123"));
        });
    }

    [TestCase("-1;1")]
    [TestCase(";1")]
    [TestCase("1;")]
    [TestCase("1;1EA3")]
    [TestCase("12.3;1")]
    [TestCase("xxxx")]
    public async Task Will_Log_On_Invalid_Reply(string reply)
    {
        /* Use the regular logger. */
        var device = new SerialPortMTRefMeter(
            SerialPortConnection.FromPortInstance(new FixedReplyMock(new[] { "ATIACK", reply, "AMEACK" }), _portLogger),
            new NullLogger<SerialPortMTRefMeter>()
        );

        /* Bad replies will only log a warning but not throw any exception. */
        await device.GetActualValues();

        /* Each log entry will create an ArgumentException. */
        Assert.ThrowsAsync<ArgumentException>(async () => await CreateDevice(new[] { "ATIACK", reply, "AMEACK" }).GetActualValues());
    }

    [Test]
    public async Task Will_Overwrite_Index_Value()
    {
        var data = await CreateDevice(new[] { "ATIACK", "28;1", "28;2", "AMEACK" }).GetActualValues();

        Assert.That(data.Frequency, Is.EqualTo(2));
    }

    [Test]
    public async Task Can_Handle_Empty_Reply()
    {
        await CreateDevice(new[] { "ATIACK", "AMEACK" }).GetActualValues();

        Assert.Pass();
    }

    [Test]
    public void Will_Detect_Missing_ACK()
    {
        Assert.ThrowsAsync<TimeoutException>(async () => await CreateDevice(new[] { "ATIACK", "0;1" }).GetActualValues());
    }

    [Test]
    public async Task Will_Cache_Request()
    {
        var device = new SerialPortMTRefMeter(SerialPortConnection.FromMock<CountingMock>(_portLogger), _deviceLogger);

        /* Since all task execute at the same time they all should get the same result. */
        var first = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => device.GetActualValues()));

        Array.ForEach(first, r => Assert.That(r.Frequency, Is.EqualTo(50)));

        Thread.Sleep(100);

        /* After all tasks complete a new request is necessary. */
        var second = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => device.GetActualValues()));

        Array.ForEach(second, r => Assert.That(r.Frequency, Is.EqualTo(51)));
    }
}
