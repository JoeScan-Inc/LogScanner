namespace JoeScan.LogScanner.Core.Helpers.CommandLine;

/// <summary>
/// A simple parser to parse Command Line Arguments.
/// </summary>
public static class CommandLineParser
{
    public static IList<CommandLineOption> ParseOptions(string[] arguments)
    {
        // Holds the Results:
        var results = new List<CommandLineOption>();

        CommandLineOption lastOption = null;

        foreach (string argument in arguments)
        {
            // What should we do here? Go to the next one:
            if (string.IsNullOrWhiteSpace(argument))
            {
                continue;
            }

            // We have found a Long-Name option:
            if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                // The previous argument was an option, too. Let's give it back:
                if (lastOption != null)
                {
                    results.Add(lastOption);
                }

                lastOption = new CommandLineOption
                {
                    OptionType = OptionTypeEnum.LongName,
                    Name = argument.Substring(2)
                };
            }
            // We have found a Short-Name option:
            else if (argument.StartsWith("-", StringComparison.Ordinal))
            {
                // The previous argument was an option, too. Let's give it back:
                if (lastOption != null)
                {
                    results.Add(lastOption);
                }

                lastOption = new CommandLineOption
                {
                    OptionType = OptionTypeEnum.ShortName,
                    Name = argument.Substring(1)
                };
            }
            // We have found a symbol:
            else if (lastOption == null)
            {
                results.Add(new CommandLineOption
                {
                    OptionType = OptionTypeEnum.Symbol,
                    Name = argument
                });
            }
            // And finally this is a value:
            else
            {
                // Set the Value and return this option:
                lastOption.Value = argument;

                results.Add(lastOption);

                // And reset it, because we do not expect multiple parameters:
                lastOption = null;
            }
        }

        if (lastOption != null)
        {
            results.Add(lastOption);
        }

        return results;
    }
}
