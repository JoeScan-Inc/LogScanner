using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IPieceNumberProvider : IDisposable
{
    int GetNextPieceNumber();
    int PeekNextPieceNumber();

    void SetNextPieceNumber(int pieceNumber);
    event EventHandler<PieceNumberChangedEventArgs> PieceNumberChanged;

}
