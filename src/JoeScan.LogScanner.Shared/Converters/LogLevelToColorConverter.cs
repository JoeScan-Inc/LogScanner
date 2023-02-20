using NLog;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace JoeScan.LogScanner.Shared.Converters;

[ValueConversion(typeof(LogLevel), typeof(SolidColorBrush))]
public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (null == value)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value is LogLevel errorValue )
        {
            if (errorValue == LogLevel.Warn)
            {
                return new SolidColorBrush(Colors.Orange);
            }

            if (errorValue >= LogLevel.Error)
            {
                return new SolidColorBrush(Colors.Red);
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
