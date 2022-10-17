using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using System.ComponentModel;
using System.Reflection;
using UnitsNet;

#pragma warning disable CS0618

namespace JoeScan.LogScanner.Desktop.LogProperties;

public class LogPropertyItemViewModel : PropertyChangedBase 
{
    public string PropertyName { get; init; } 
    public string PropertyValue { get; private set; } = "";
    public string UnitString { get; private set; } = "";
   
    
    public LogPropertyItemViewModel(string propertyName)
    {
       
    }

    public void UpdateWith(LogData ld)
    {
       
       
    }

    private static string SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
}
