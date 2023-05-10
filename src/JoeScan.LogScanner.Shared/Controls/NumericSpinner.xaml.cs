using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace JoeScan.LogScanner.Shared.Controls
{
    /// <summary>
    /// This spinner control is custom, put together using bits and pieces from other projects (all MIT LICENSED)
    /// Original Project: https://github.com/Stopbyte/WPF-Numeric-Spinner-NumericUpDown
    /// </summary>
    public partial class NumericSpinner : UserControl
    {
        #region Fields

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged",
            RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double?>), typeof(NumericSpinner));

        #endregion

        public NumericSpinner()
        {
            InitializeComponent();

            mText.SetBinding(TextBox.TextProperty, new Binding("Value")
            {
                ElementName = "root_numeric_spinner",
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            mText.LostFocus += OnLostFocus;
            mText.KeyDown += OnKeyDown;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var numericSpinner = (NumericSpinner)d;
            numericSpinner.OnValueChanged((double?)e.OldValue, (double?)e.NewValue);
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(((TextBox)sender).Text, out double convertedValue))
            {
                Value = convertedValue;
            }

            mText.Text = Value.ToString(StringFormat);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                OnLostFocus(sender, new RoutedEventArgs());
            }
        }

        private static object CoerceValue(DependencyObject d, object value)
        {
            if (value == null)
            {
                return null;
            }

            var numericUpDown = (NumericSpinner)d;
            double val = ((double?)value).Value;

            if (val < numericUpDown.MinValue)
            {
                return numericUpDown.MinValue;
            }

            if (val > numericUpDown.MaxValue)
            {
                return numericUpDown.MaxValue;
            }

            return val;
        }

        protected virtual void OnValueChanged(double? oldValue, double? newValue)
        {
            if (!newValue.HasValue)
            {
                if (mText != null)
                {
                    mText.Text = null;
                }

                return;
            }

            if (mText != null)
            {
                mText.Text = newValue.Value.ToString(StringFormat);
            }

            if (oldValue != newValue)
            {
                RaiseEvent(new RoutedPropertyChangedEventArgs<double?>(oldValue, newValue, ValueChangedEvent));
            }
        }

        #region ValueProperty

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double?),
            typeof(NumericSpinner),
            new FrameworkPropertyMetadata(default(double?), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceValue, false, UpdateSourceTrigger.Explicit));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set
            {
                if (value == Value)
                {
                    return;
                }

                if (value > MaxValue)
                {
                    value = MaxValue;
                }

                if (value < MinValue)
                {
                    value = MinValue;
                }

                SetValue(ValueProperty, value);
            }
        }

        #endregion

        #region IntervalProperty

        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register(
            "Interval",
            typeof(double),
            typeof(NumericSpinner),
            new PropertyMetadata(1.0));

        public double Interval
        {
            get => (double)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        #endregion

        #region MinValueProperty

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            "MinValue",
            typeof(double),
            typeof(NumericSpinner),
            new PropertyMetadata(double.MinValue));

        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set
            {
                if (value > MaxValue)
                {
                    MaxValue = value;
                }

                SetValue(MinValueProperty, value);
            }
        }

        #endregion

        #region MaxValueProperty

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            "MaxValue",
            typeof(double),
            typeof(NumericSpinner),
            new PropertyMetadata(double.MaxValue));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set
            {
                if (value < MinValue)
                {
                    value = MinValue;
                }

                SetValue(MaxValueProperty, value);
            }
        }

        #endregion

        #region StringFormatProperty

        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            "StringFormat",
            typeof(string),
            typeof(NumericSpinner),
            new PropertyMetadata(string.Empty));

        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }

        #endregion

        private void CmdUp_Click(object sender, RoutedEventArgs e)
        {
            if (mText.Text.Length > 0)
            {
                Value += Interval;
            }
        }

        private void CmdDown_Click(object sender, RoutedEventArgs e)
        {
            if (mText.Text.Length > 0)
            {
                Value -= Interval;
            }
        }
    }
}
