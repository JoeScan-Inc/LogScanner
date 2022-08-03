using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using System.ComponentModel;

namespace JoeScan.LogScanner.LogReview.Interfaces;

public interface ILogModelObservable : INotifyPropertyChanged
{
    public string CurrentFile { get; }
    public RawLog? CurrentRawLog { get;  }
    public LogModel? CurrentLogModel { get; }
    public IObservableCollection<LogSection>? Sections { get; }
    public LogSection? CurrentSection { get; set; }

    public void Load(string file);

}
