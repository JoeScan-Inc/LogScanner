using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

namespace RawViewer.Converters;

[ValueConversion(typeof(uint), typeof(string))]
public class CameraIndexToStringConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (null == value)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is uint cam)
        {
            switch (cam)
            {
                case 1:
                    return "A";
                case 2: 
                    return "B";
                default:
                    return cam.ToString();
            }
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
