using JoeScan.LogScanner.Core.Events;
using JoeScan.LogScanner.Core.Extensions;
using NLog;
using System.Net;
using System.Net.Sockets;

namespace JoeScan.LogScanner.Js50;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public  class ScanSyncReceiverThread : IDisposable
{
    private readonly ILogger logger;

    #region Private Fields
    // TODO: get them from the Pinchot API
    private const int ScanSyncClientPort = 11234;
    private const int ScanSyncServerPort = 62510;

    private long bytesReceived;
    private long goodPackets;
    private long badPackets;
    private readonly UdpClient receiverClient;
    private IPEndPoint groupEndPoint;
    private CancellationTokenSource? cancellationTokenSource;
    private CancellationToken token;
    private Thread? threadMain;
    private int counter;
    private bool disposed;

    #endregion

    #region Internal Properties

    internal int EventUpdateFrequency { get; set; } = 1000;

    #endregion

    #region Events

    internal event EventHandler<EncoderUpdateArgs>? ScanSyncUpdate;

    #endregion

    #region Lifecycle

    public ScanSyncReceiverThread(ILogger logger)
    {
        this.logger = logger;
        receiverClient = new UdpClient(new IPEndPoint(IPAddress.Any, ScanSyncClientPort));
        groupEndPoint = new IPEndPoint(IPAddress.Any, ScanSyncServerPort);

    }
    ~ScanSyncReceiverThread()
    {
        Dispose(false);
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by the <see cref="ScanSyncReceiverThread"/>
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ScanSyncReceiverThread"/> and optionally
    /// releases the managed resources.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            if (cancellationTokenSource != null)
            {
                threadMain!.Join();
                cancellationTokenSource.Cancel();
                cancellationTokenSource?.Dispose();
            }
            receiverClient?.Dispose();
        }
        disposed = true;
    }

    #endregion

    #region Internal Methods

    internal void Start()
    {
        if (cancellationTokenSource == null)
        {
            logger.Debug("Starting ScanSyncReceiverThread");
            cancellationTokenSource = new CancellationTokenSource();
            token = cancellationTokenSource.Token;
            bytesReceived = 0L;
            goodPackets = 0L;
            badPackets = 0L;
            threadMain = new Thread(ThreadMain) { Priority = ThreadPriority.BelowNormal, IsBackground = true };
            threadMain.Start();
        }
    }

    internal void Stop()
    {
        if (cancellationTokenSource != null)
        {
            logger.Debug("Stopping ScanSyncReceiverThread");

            cancellationTokenSource.Cancel();
            threadMain!.Join();
            cancellationTokenSource = null;
            logger.Debug("ScanSyncReceiverThread stopped.");

        }
    }

    #endregion

    #region Private Methods

    private void ThreadMain()
    {
        bytesReceived = 0;
        // this callback will kill the socket when the
        // token was canceled, which is the only way to get out
        // of the blocking udpClient.Receive()
        token.Register(() => receiverClient.Close());
        for (; ; )
        {
            try
            {
                token.ThrowIfCancellationRequested();

                // raw scansync packet
                byte[] rsp = receiverClient.Receive(ref groupEndPoint);
                goodPackets++;
                bytesReceived += rsp.Length;
                if (counter++ == EventUpdateFrequency)
                {
                    var packet = new ScanSyncPacket(rsp);
                    ScanSyncUpdate.Raise(this, new EncoderUpdateArgs(
                        packet.ScanSyncData.SerialNumber,
                        packet.ScanSyncData.Sequence,
                        packet.ScanSyncData.EncoderTimeStampSeconds,
                        packet.ScanSyncData.EncoderTimeStampNanoseconds,
                        packet.ScanSyncData.LastTimeStampSeconds,
                        packet.ScanSyncData.LastTimeStampNanoseconds,
                        packet.ScanSyncData.EncoderValue,
                        badPackets));
                    counter = 0;
                }
            }
            catch (ArgumentException)
            {
                if (badPackets < 100)
                {
                    // avoid choking the log
                    logger.Trace("Received bad packet. Ignoring.");
                }
                badPackets++;
            }
            catch (OperationCanceledException)
            {
                // perfectly normal, nothing to see here
                break;
            }
            catch (SocketException)
            {
                // we get here if we call Close() on the UdpClient
                // while it is in the Receive(0 call. Apparently
                // the only way to abort a Receive call is to
                // close the underlying Socket.
                break;
            }
            catch (Exception)
            {
                // Receive failed.
                break;
            }
        }
    }

    #endregion

    private class ScanSyncPacket
    {
        private readonly byte[] raw;

        public ScanSyncPacket(byte[] raw)
        {
            if (raw.Length != 32)
            {
                throw new ArgumentException("Raw ScanSync Packet invalid.");
            }

            this.raw = raw;
        }

        internal ScanSyncData ScanSyncData =>
            new ScanSyncData()
            {
                SerialNumber = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 0)),
                Sequence = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 4)),
                EncoderTimeStampSeconds = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 8)),
                EncoderTimeStampNanoseconds = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 12)),
                LastTimeStampSeconds = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 16)),
                LastTimeStampNanoseconds = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(raw, 20)),
                EncoderValue = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(raw, 24))
            };

    }

    private class ScanSyncData
    {
        public int SerialNumber { get; internal set; }
        public int Sequence { get; internal set; }
        public int EncoderTimeStampSeconds { get; internal set; }
        public int EncoderTimeStampNanoseconds { get; internal set; }
        public int LastTimeStampSeconds { get; internal set; }
        public int LastTimeStampNanoseconds { get; internal set; }
        public long EncoderValue { get; internal set; }
    }
}

