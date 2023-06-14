namespace JoeScan.LogScanner.Core.Helpers.CommandLine;

public enum OptionTypeEnum
{
    /// <summary>
    /// A Long Name for an Option, e.g. --opt.
    /// </summary>
    LongName,

    /// <summary>
    /// A Short Name for an Option, e.g. -o.
    /// </summary>
    ShortName,

    /// <summary>
    /// A Symbol, that is neither a switch, nor an argument.
    /// </summary>
    Symbol
}
