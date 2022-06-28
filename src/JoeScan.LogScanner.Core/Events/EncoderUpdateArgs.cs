namespace JoeScan.LogScanner.Core.Events;

public class EncoderUpdateArgs : EventArgs
{
    public int SerialNumber { get; }
    public int Sequence { get; }
    public int EncoderTimeStampSeconds { get; }
    public int EncoderTimeStampNanoseconds { get; }
    public int LastTimeStampSeconds { get; }
    public int LastTimeStampNanoseconds { get; }
    public long EncoderValue { get; }
    public long BadPacketCount { get; }

    public EncoderUpdateArgs(int serialNumber, int sequence, int encoderTimeStampSeconds, int encoderTimeStampNanoseconds, int lastTimeStampSeconds, int lastTimeStampNanoseconds, long encoderValue, long badPacketCount)
    {
        SerialNumber = serialNumber;
        Sequence = sequence;
        EncoderTimeStampSeconds = encoderTimeStampSeconds;
        EncoderTimeStampNanoseconds = encoderTimeStampNanoseconds;
        LastTimeStampSeconds = lastTimeStampSeconds;
        LastTimeStampNanoseconds = lastTimeStampNanoseconds;
        EncoderValue = encoderValue;
        BadPacketCount = badPacketCount;
    }
}
