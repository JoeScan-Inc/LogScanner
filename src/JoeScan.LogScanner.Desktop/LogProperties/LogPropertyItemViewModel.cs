using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using System;
// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable CS0618

namespace JoeScan.LogScanner.Desktop.LogProperties;

public class LogPropertyItemViewModel : PropertyChangedBase
{
    private readonly Func<LogModel, string> displayFunc;
    public string PropertyName { get; init; }
    public string PropertyValue { get; private set; } = "";
    public string UnitString { get; private set; } = "";
    
    
    public LogPropertyItemViewModel(string propertyName, string unit, Func<LogModel, string> displayFunc)
    {
        this.displayFunc = displayFunc;
        PropertyName = propertyName;
        UnitString = unit;
    }

    public void UpdateWith(LogModel model)
    {
        PropertyValue = displayFunc(model) ;
        Refresh();
    }
}
