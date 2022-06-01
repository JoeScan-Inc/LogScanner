namespace JoeScan.LogScanner.Core.Enums;
/// <summary>
/// these are the states the LogAssembler state machine. 
/// </summary>
public enum LogAssemblerState
{
    /// <summary>
    /// The state machine is in Idle when starting up, and remains there until enough
    /// valid profiles have been collected to enter the Tentative state. 
    /// </summary>
    Idle,
    /// <summary>
    /// In Tentative state, we collect profiles in a separate buffer until we have enough
    /// valid profiles to be sure that this really is a log start. If stray data caused us
    /// to go into tentative state, and empty profiles follow, we fall back into Idle
    /// </summary>
    Tentative,
    /// <summary>
    /// In this state, we are actively collecting all profiles, until the end-of-log is
    /// reached and we drop back to idle.
    /// </summary>
    Collecting
}
