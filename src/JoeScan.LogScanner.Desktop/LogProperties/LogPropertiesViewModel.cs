using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;

namespace JoeScan.LogScanner.Desktop.LogProperties;

public class LogPropertiesViewModel : Screen
{
    public IObservableCollection<LogPropertyItemViewModel> Items { get; set; }
        = new BindableCollection<LogPropertyItemViewModel>();

    public LogPropertiesViewModel()
    {
        // get all properties on LogData that are decorated with the [Unit] attribute
        foreach (var propertyInfo in typeof(LogData).GetProperties())
        {
            object[] attribute = propertyInfo.GetCustomAttributes(typeof(UnitAttribute), true);
            if (attribute.Length > 0)
            {
                Items.Add(new LogPropertyItemViewModel(propertyInfo));
            }
        }


        foreach (var item in Items)
        {
            item.UpdateWith(new LogData(){ Length = 42.0});
        }

    }
}
