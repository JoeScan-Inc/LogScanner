using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Core.Interfaces;

public interface IRawProfileValidator
{
    // this is where we decide if this profile contains valid data or just flights
    // and stray points
    bool IsValid(Profile p);

}
