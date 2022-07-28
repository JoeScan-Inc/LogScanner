using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using JoeScan.LogScanner.LogReview.Models;
using JoeScan.LogScanner.LogReview.Navigator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JoeScan.LogScanner.LogReview.SectionTable;

public class SectionTableViewModel : Screen
{
    private LogModel? currentLogModel;
    private LogSection currentSection;
    public event EventHandler? SectionChanged;
    public SectionTableViewModel()
    {

    }

    public IObservableCollection<LogSection> Sections { get; set; } = new BindableCollection<LogSection>();

    public LogModel? CurrentLogModel
    {
        get => currentLogModel;
        set
        {
            if (Equals(value, currentLogModel))
            {
                return;
            }
            currentLogModel = value;
            NotifyOfPropertyChange(() => CurrentLogModel);
            Sections.Clear();
            if (currentLogModel != null)
            {
                Sections.AddRange(currentLogModel.Sections);
            }

            Refresh();
        }
    }

    public LogSection? CurrentSection
    {
        get => currentSection;
        set
        {
            if (value == currentSection)
            {
                return;
            }
            currentSection = value;
            OnSectionChanged();
            Refresh();
        }
    }


    protected virtual void OnSectionChanged()
    {
        SectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
