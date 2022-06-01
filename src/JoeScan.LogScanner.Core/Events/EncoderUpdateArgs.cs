namespace JoeScan.LogScanner.Core.Events;

public class EncoderUpdateArgs : EventArgs
{
    public long EncoderValue { get; internal set; }
    public ulong TimeStamp { get; internal set; }

    public EncoderUpdateArgs(long encoderValue, ulong timeStamp)
    {
        EncoderValue = encoderValue;
        TimeStamp = timeStamp;
    }
}
