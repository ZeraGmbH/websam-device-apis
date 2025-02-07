using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SerialPortProxy;
using ZERA.WebSam.Shared.Models;
using ZERA.WebSam.Shared.Models.Logging;
using ZIFApi.Exceptions;
using ZIFApi.Models;

namespace ZIFApi.Actions;

/// <summary>
/// 
/// </summary>
public class PowerMaster8121(IPortSetup821xVSW configLoader, ILogger<PowerMaster8121> _logger) : IZIFProtocol
{
    private readonly struct PortKey(string meterForm, string serviceType)
    {
        public readonly string MeterForm = meterForm;

        public readonly string ServiceType = serviceType;

        public override bool Equals(object? obj)
            => obj is PortKey other && MeterForm.Equals(other.MeterForm) && ServiceType.Equals(other.ServiceType);

        public override int GetHashCode()
            => MeterForm.GetHashCode() ^ ServiceType.GetHashCode();

        public override string ToString()
            => $"{MeterForm}:{ServiceType}";
    }

    private static readonly byte[] _crcTable =
    [
        0,  94, 188, 226,  97,  63, 221, 131, 194, 156, 126,  32, 163, 253,  31,  65,
        157, 195,  33, 127, 252, 162,  64,  30,  95,   1, 227, 189,  62,  96, 130, 220,
        35, 125, 159, 193,  66,  28, 254, 160, 225, 191,  93,   3, 128, 222,  60,  98,
        190, 224,   2,  92, 223, 129,  99,  61, 124,  34, 192, 158,  29,  67, 161, 255,
        70,  24, 250, 164,  39, 121, 155, 197, 132, 218,  56, 102, 229, 187,  89,   7,
        219, 133, 103,  57, 186, 228,   6,  88,  25,  71, 165, 251, 120,  38, 196, 154,
        101,  59, 217, 135,   4,  90, 184, 230, 167, 249,  27,  69, 198, 152, 122,  36,
        248, 166,  68,  26, 153, 199,  37, 123,  58, 100, 134, 216,  91,   5, 231, 185,
        140, 210,  48, 110, 237, 179,  81,  15,  78,  16, 242, 172,  47, 113, 147, 205,
        17,  79, 173, 243, 112,  46, 204, 146, 211, 141, 111,  49, 178, 236,  14,  80,
        175, 241,  19,  77, 206, 144, 114,  44, 109,  51, 209, 143,  12,  82, 176, 238,
        50, 108, 142, 208,  83,  13, 239, 177, 240, 174,  76,  18, 145, 207,  45, 115,
        202, 148, 118,  40, 171, 245,  23,  73,   8,  86, 180, 234, 105,  55, 213, 139,
        87,   9, 235, 181,  54, 104, 138, 212, 149, 203,  41, 119, 244, 170,  72,  22,
        233, 183,  85,  11, 136, 214,  52, 106,  43, 117, 151, 201,  74,  20, 246, 168,
        116,  42, 200, 150,  21,  75, 169, 247, 182, 232,  10,  84, 215, 137, 107,  53
    ];

    /// <inheritdoc/>
    public int Index { get; set; }

    /// <inheritdoc/>
    public int? ReadTimeout { get; set; }

    /// <summary>
    /// Calculate a CRC8(MAXIM)
    /// </summary>
    /// <param name="data">Some data.</param>
    /// <returns>Requested checkum over data.</returns>
    public static byte ComputeCRC(IEnumerable<byte> data)
    {
        byte crc = 0;

        foreach (var dt in data)
            crc = _crcTable[(byte)(crc ^ dt)];

        return crc;
    }

    private enum ReadState { Start, End, Length, Data, Checksum };

    private async Task ExecuteAsync(ISerialPortConnection factory, IInterfaceLogger logger, params int[] command)
        => await Execute(factory, logger, response => response.Length == 0 ? true : throw new BadLengthException(0), command);

    /// <summary>
    /// Convert a command to a protocol request including checksum.
    /// </summary>
    /// <param name="command">Command to send.</param>
    /// <returns>Protocol representation.</returns>
    public static byte[] CommandToProtocol(params int[] command)
    {
        // - STX and length of command bytes.
        var buffer = new List<byte> { 0xa5, checked((byte)(1 + command.Length)) };

        // - Command bytes.
        buffer.AddRange(command.Select(b => checked((byte)b)));

        // - CRC8(MAXIM) checksum.
        buffer.Add(ComputeCRC(buffer.Skip(1)));

        // - ETX
        buffer.Add(0x5a);

        return [.. buffer];
    }

    private Task<T> Execute<T>(ISerialPortConnection factory, IInterfaceLogger logger, Func<byte[], T> createResponse, params int[] command)
        => Execute(factory, $"{Index}", ReadTimeout, logger, _logger, createResponse, command);

    /// <summary>
    /// Execute a single command.
    /// </summary>
    /// <param name="factory">Port factory to use.</param>
    /// <param name="interfaceLogger">Interface logging.</param>
    /// <param name="logger">System logging.</param>
    /// <param name="createResponse">How to create the response from the binary data.</param>
    /// <param name="id">Test position of the socket.</param>
    /// <param name="readTimeout">Maximum time to wait for reply in milliseconds.</param>
    /// <param name="command">Command to execute.</param>
    /// <typeparam name="T">Type of response.</typeparam>
    /// <returns>Response reconstructed from binary reply.</returns>
    public static Task<T> Execute<T>(ISerialPortConnection factory, string id, int? readTimeout, IInterfaceLogger interfaceLogger, ILogger logger, Func<byte[], T> createResponse, params int[] command)
    {
        var buffer = CommandToProtocol(command);

        // It's now time to get exclusive access to the serial port.
        return factory.CreateExecutor(InterfaceLogSourceTypes.ZIF, id).RawExecuteAsync(interfaceLogger, (port, connection, cancel) =>
        {
            /* Prepare logging. */
            var requestId = Guid.NewGuid().ToString();

            IPreparedInterfaceLogEntry sendEntry = null!;
            Exception sendError = null!;

            try
            {
                /* Start logging. */
                sendEntry = connection.Prepare(new() { Outgoing = true, RequestId = requestId });

                /* Send to device. */
                port.RawWrite(buffer);
            }
            catch (Exception e)
            {
                logger.LogError("Unable to sent {Command} to port: {Exception}", BitConverter.ToString([.. command.Select(b => (byte)b)]), e.Message);

                /* Data could not be sent - create a log entry for the issue. */
                sendError = e;
            }

            /* Finish logging. */
            if (sendEntry != null)
                try
                {
                    sendEntry.Finish(new()
                    {
                        Encoding = InterfaceLogPayloadEncodings.Raw,
                        Payload = BitConverter.ToString(buffer),
                        PayloadType = "",
                        TransferException = sendError?.Message
                    });
                }
                catch (Exception e)
                {
                    /* Nothing more we can do. */
                    logger.LogCritical("Unable to create log entry: {Exception}", e.Message);
                }

            /* Failed - we can stop now. */
            if (sendError != null) throw new SendException(sendError);

            IPreparedInterfaceLogEntry receiveEntry = null!;
            Exception receiveError = null!;
            List<byte> reply = [];

            try
            {
                /* Start logging. */
                receiveEntry = connection.Prepare(new() { Outgoing = false, RequestId = requestId });

                /* Retrieve the answer from the socket and convert to a model. */
                return createResponse(ReadResponse(command, port, reply, readTimeout ?? 30000, cancel));
            }
            catch (Exception e)
            {
                logger.LogError("Unable to read reply for {Command} from port: {Exception}", BitConverter.ToString([.. command.Select(b => (byte)b)]), e.Message);

                /* Reply could not be received - create a log entry for the issue. */
                receiveError = e;

                throw new ReceiveException(e);
            }
            finally
            {
                /* Finish logging. */
                if (receiveEntry != null)
                    try
                    {
                        receiveEntry.Finish(new()
                        {
                            Encoding = InterfaceLogPayloadEncodings.Raw,
                            Payload = reply == null ? string.Empty : BitConverter.ToString([.. reply]),
                            PayloadType = "",
                            TransferException = receiveError?.Message
                        });
                    }
                    catch (Exception e)
                    {
                        /* Nothing more we can do. */
                        logger.LogCritical("Unable to create log entry: {Exception}", e.Message);
                    }
            }
        });
    }

    /// <summary>
    /// Read response for a command.
    /// </summary>
    /// <param name="command">Command sent.</param>
    /// <param name="port">Serial port connection to use.</param>
    /// <param name="all">Everything we got.</param>
    /// <param name="readTimeout">Timeout to read the response.</param>
    /// <param name="cancel">Manages cancel requests.</param>
    /// <returns>Payload.</returns>
    public static byte[] ReadResponse(int[] command, ISerialPort port, List<byte> all, int readTimeout, ICancellationService? cancel)
    {
        var data = new List<byte>();
        int length = 0;

        for (var state = ReadState.Start; ;)
        {
            var next = port.RawRead(readTimeout, cancel) ?? throw new NoMoreDataException();

            all.Add(next);

            switch (state)
            {
                case ReadState.Start:
                    /* Wait for STX, restart on any garbage. */
                    if (next == 0xa5) state = ReadState.Length;

                    break;
                case ReadState.Length:
                    /* Expect a length - includes itself in the data so must be at least 1. */
                    if (next < 1)
                        state = ReadState.Start;
                    else
                    {
                        data.Clear();
                        data.Add(next);

                        length = next;

                        /* Normally may want to parse for data but support no data reply just in case. */
                        state = --length == 0 ? ReadState.Checksum : ReadState.Data;
                    }

                    break;
                case ReadState.Data:
                    /* Collect data as required in length. */
                    data.Add(next);

                    if (--length == 0) state = ReadState.Checksum;

                    break;
                case ReadState.Checksum:
                    /* On checksum mismatch start again from STX. */
                    var crc = ComputeCRC(data);

                    state = crc == next ? ReadState.End : ReadState.Start;

                    break;
                case ReadState.End:
                    /* Wait for ETX - restart on mismatch. */
                    if (next != 0x5a)
                    {
                        state = ReadState.Start;

                        break;
                    }

                    return data[1] switch
                    {
                        /* ACK - just report the payload itself. */
                        0x06 =>
                            /* Current protocol requires to have the incoming command as the second data position - [0] is the length, convienent kept in to easy CRC calculation. */
                            (data.Count >= 3)
                                ? data[2] == command[0]
                                    ? [.. data.Skip(3)]
                                    : throw new OutOfBandException(data[2], (byte)command[0])
                                : throw new ReplyToShortException(),
                        /* NAK. */
                        0x15 => throw new NAKException([.. data.Skip(3)]),
                        /* Currently unsupported. */
                        _ => throw new ACKorNAKException(data[1]),
                    };
            }
        }
    }

    /// <inheritdoc/>

    public Task<ZIFVersionInfo> GetVersionAsync(ISerialPortConnection factory, IInterfaceLogger logger)
        => Execute(
            factory,
            logger,
            response => response.Length == 5
                ? new ZIFVersionInfo { Protocol = SupportedZIFProtocols.PowerMaster8121, Major = BitConverter.ToInt32(response), Minor = response[4] }
                : throw new BadLengthException(5),
            0xc2
        );

    /// <inheritdoc/>
    public Task<int> GetSerialAsync(ISerialPortConnection factory, IInterfaceLogger logger)
        => Execute(
                factory,
                logger,
                response => response.Length == 2
                    ? response[1] + 256 * response[0]
                    : throw new BadLengthException(2),
                0xc1
            );

    /// <inheritdoc/>
    public Task<bool> GetActiveAsync(ISerialPortConnection factory, IInterfaceLogger logger)
        => Execute(
                factory,
                logger,
                response => response.Length == 1
                    ? (response[0] & 0x01) == 0x01
                    : throw new BadLengthException(1),
                0xc4
            );

    /// <inheritdoc/>
    public Task<bool> GetHasErrorAsync(ISerialPortConnection factory, IInterfaceLogger logger)
        => Execute(
                factory,
                logger,
                response => response.Length == 1
                    ? (response[0] & 0x40a) == 0x40
                    : throw new BadLengthException(1),
                0xc4
            );

    /// <inheritdoc/>
    public Task<bool> GetHasMeterAsync(ISerialPortConnection factory, IInterfaceLogger logger)
        => Execute(
                factory,
                logger,
                response => response.Length == 1
                    ? (response[0] & 0x02) == 0x02
                    : throw new BadLengthException(1),
                0xc4
            );

    /// <inheritdoc/>
    public Task SetActiveAsync(bool active, ISerialPortConnection factory, IInterfaceLogger logger)
        => ExecuteAsync(
                factory,
                logger,
                0x8d, active ? 0x01 : 0x00
            );

    /// <inheritdoc/>
    public async Task SetMeterAsync(string meterForm, string serviceType, ISerialPortConnection factory, IInterfaceLogger logger)
    {
        /* See if we support this meter. */
        var setups = await configLoader.PortSetups;

        if (!setups.TryGetValue(new(meterForm, serviceType), out var setup))
            throw new BadMeterFormServiceTypeCombinationException(meterForm, serviceType);

        await ExecuteAsync(
            factory,
            logger,
            0x8e,
            PortSetup821xVSW.PortMasks[0], setup[0],
            PortSetup821xVSW.PortMasks[1], setup[1],
            PortSetup821xVSW.PortMasks[2], setup[2],
            PortSetup821xVSW.PortMasks[3], setup[3],
            PortSetup821xVSW.PortMasks[4], setup[4],
            PortSetup821xVSW.PortMasks[5], setup[5],
            PortSetup821xVSW.PortMasks[6], setup[6]
        );
    }
}
