namespace JoeScan.LogScanner.Core.Interfaces;

public interface IPieceNumberProvider : IDisposable
{
    int GetNextPieceNumber();
}
