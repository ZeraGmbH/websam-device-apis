using System.Text.RegularExpressions;

using SerialPortProxy;

namespace SourceApi.Actions.SerialPort.FG30x;

/// <summary>
/// 
/// </summary>
public class SerialPortFGMock : ISerialPort
{
    private static readonly Regex UpCommand = new(@"^UP([EA])([EA])R(\d{3}\.\d{3})(\d{3}\.\d{2})S(\d{3}\.\d{3})(\d{3}\.\d{2})T(\d{3}\.\d{3})(\d{3}\.\d{2})$");

    private static readonly Regex IpCommand = new(@"^IP([EA])([AM])R(\d{3}\.\d{3})(\d{3}\.\d{2})S(\d{3}\.\d{3})(\d{3}\.\d{2})T(\d{3}\.\d{3})(\d{3}\.\d{2})$");

    private static readonly Regex FrCommand = new(@"^FR(\d{2}\.\d{2})$");

    private static readonly Regex UiCommand = new(@"^UI([AE])([AE])([AE])([AP])([AP])([AP])([AE])([AE])([AE])$");

    private static readonly Regex ZpCommand = new(@"^ZP\d{10}$");

    private static readonly Regex MaCommand = new(@"^MA(.+)$");

    private readonly Queue<QueueEntry> _replies = new();

    /// <inheritdoc/>
    public virtual void Dispose()
    {
    }

    /// <inheritdoc/>
    public virtual string ReadLine()
    {
        if (!_replies.TryDequeue(out var info))
            throw new TimeoutException("no reply in queue");

        return info.Reply;
    }

    /// <summary>
    /// Generate a random factor.
    /// </summary>
    /// <param name="number"></param>
    /// <returns>Number scaled wit a random factor between -1% and +1%.</returns>
    private static double MockNumber(double number) => number * (1.0 + Random.Shared.Next(-1000, +1000) / 100000.0);

    /// <inheritdoc/>
    public virtual void WriteLine(string command)
    {
        switch (command)
        {
            case "TS":
                _replies.Enqueue("TSFG301   V299");
                break;
            case "MI":
                _replies.Enqueue("MI4LW;3LW;4LBK;4LBE;3LBK;3LBA;3BKB;3LBE;3LWR;4LBF;1PHT;1PHR;1PHA;4LS;3LS;4LQ6;3LQ6;4Q6K;3Q6K;4LSG;3LSG;4LBG;3LBG;");
                break;
            case "BU":
                /* Range Voltage: 250V */
                _replies.Enqueue("BU250.000");
                break;
            case "AU":
                /* Voltage: 200V, 250V, 300V */
                _replies.Enqueue($"AUR{MockNumber(16000):00000}S{MockNumber(20000):00000}T{MockNumber(24000):00000}");
                break;
            case "BI":
                // Range Current: 5A */
                _replies.Enqueue("BI5.000");
                break;
            case "AI":
                /* Current: 2A, 1A, 3A */
                _replies.Enqueue($"AIR{MockNumber(8000):00000}S{MockNumber(4000):00000}T{MockNumber(12000):00000}");
                break;
            case "AW":
                /* Angle (V,A): (0, 5), (110, 130), (245, 231) */
                _replies.Enqueue($"AWR{MockNumber(0):000.0}{MockNumber(5):000.0}S{MockNumber(110):000.0}{MockNumber(130):000.0}T{MockNumber(245):000.0}{MockNumber(231):000.0}");
                break;
            case "MP":
                /* Active Power: 400W, 235W, 900W => 1535W */
                _replies.Enqueue($"MPR{MockNumber(400):0.0};S{MockNumber(235):0.0};T{MockNumber(900):0.0}");
                break;
            case "MQ":
                /* Reactive Power: 35var, 85var, -80var => 40var */
                _replies.Enqueue($"MQR{MockNumber(35):0.0};S{MockNumber(85):0.0};T{MockNumber(-80):0.0}");
                break;
            case "MS":
                /* Apparent power: 400VA, 250VA, 900VA => 1550VA */
                _replies.Enqueue($"MSR{MockNumber(400):0.0};S{MockNumber(250):0.0};T{MockNumber(900):0.0}");
                break;
            case "AF":
                /* Frequency: 50Hz */
                _replies.Enqueue($"AF{MockNumber(50.0):00.00}");
                break;
            default:
                {
                    /* Set voltage. */
                    if (UpCommand.IsMatch(command))
                        _replies.Enqueue("OKUP");
                    /* Set current.*/
                    else if (IpCommand.IsMatch(command))
                        _replies.Enqueue("OKIP");
                    /* Set frequency. */
                    else if (FrCommand.IsMatch(command))
                        _replies.Enqueue("OKFR");
                    /* Activate phases. */
                    else if (UiCommand.IsMatch(command))
                        _replies.Enqueue("OKUI");
                    /* Configure amplifiers and reference meter. */
                    else if (ZpCommand.IsMatch(command))
                        _replies.Enqueue("OKZP");
                    /** Set the measuring mode. */
                    else if (MaCommand.IsMatch(command))
                        _replies.Enqueue("OKMA");

                    break;
                }

        }
    }
}
