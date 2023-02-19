using Caliburn.Micro;
using JoeScan.LogScanner.LogReview.Config;
using JoeScan.LogScanner.Shared.Enums;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using UnitsNet.Units;

namespace JoeScan.LogScanner.LogReview.Converters;
[ValueConversion(typeof(double), typeof(string))]
public class DisplayUnitConverter : IValueConverter
{
    private LengthUnit displayUnits;

    public DisplayUnitConverter()
    {
        var config = IoC.Get<ILogReviewConfig>();
        displayUnits = config.Units == DisplayUnits.Inches ? LengthUnit.Inch : LengthUnit.Millimeter;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (null == value)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is double errorValue && !double.IsNaN(errorValue))
        {
            return $"{UnitsNet.Length.FromMillimeters(errorValue).ToUnit(displayUnits):F2}";
        }

        // the binding engine interprets this as a binding failure and will use
        // the default value for the bound property or a fallback, if present
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
