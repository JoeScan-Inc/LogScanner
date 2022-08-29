using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using System.ComponentModel;
using System.Reflection;
using UnitsNet;

#pragma warning disable CS0618

namespace JoeScan.LogScanner.Desktop.LogProperties;

public class LogPropertyItemViewModel : PropertyChangedBase 
{
    private readonly PropertyInfo property;
    public string PropertyName { get; init; } 
    public string PropertyValue { get; private set; } = "";
    public string UnitString { get; private set; } = "";
    private QuantityType SourceQuantityType { get; set; } = QuantityType.Undefined;
    private IQuantity? quantity;
    
    public LogPropertyItemViewModel(PropertyInfo property)
    {
        this.property = property;
        object[] attribute = property.GetCustomAttributes(typeof(UnitAttribute), true);
        if (attribute.Length > 0)
        {
            var attr = (UnitAttribute)attribute[0];
            SourceQuantityType= attr.SourceUnitType;
        }
        attribute = property.GetCustomAttributes(typeof(DisplayNameAttribute), true);
        if (attribute.Length > 0)
        {
            DisplayNameAttribute myAttribute = (DisplayNameAttribute)attribute[0];
            PropertyName = myAttribute.DisplayName;
        }
        else
        {
            PropertyName = SplitCamelCase(property.Name);
        }
    }

    public void UpdateWith(LogData ld)
    {
        var val = property.GetValue(ld);
        if (val == null)
        {
            PropertyValue = "n/a";
            UnitString = "";
        }
        if (SourceQuantityType != QuantityType.Undefined)
        {
            
            switch (SourceQuantityType)
            {
                case QuantityType.Length:
                    quantity = Quantity.From((double)val, ld.SourceLengthUnit);
                    break;
                case QuantityType.Volume:
                    quantity = Quantity.From((double)val, ld.SourceVolumeUnit);
                    break;
                case QuantityType.Angle:
                    quantity = Quantity.From((double)val, ld.SourceAngleUnit);
                    break;
                default:
                    quantity = null;
                    break;
            }
        }
        else
        {
            // we have a property in unknown units, so we use ToString only
        }
    }

    private static string SplitCamelCase(string input)
    {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
}
