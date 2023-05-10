using Caliburn.Micro;
using JoeScan.LogScanner.Core.Models;
using NLog;
using RawViewer.Helpers;
using RawViewer.Shell;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RawViewer;

public class DataManager : PropertyChangedBase
{
    public event EventHandler ProfileDataAdded;
    public event EventHandler HeadSelectionChanged;

    private RawProfile? selectedProfile;
    public ILogger Logger { get; }
    public IObservableCollection<RawProfile> Profiles { get; } = new BindableCollection<RawProfile>();
    

    public ObservableCollection<KeyValuePair<int, string>> SelectableHeads { get; } =
        new ObservableCollection<KeyValuePair<int, string>>();
    private object _lock = new object();
    private readonly ICollectionView filteredView;
    private int scanHeadFilterById = -1;
    private double encoderPulseInterval;

    public int ScanHeadFilterById
    {
        get => scanHeadFilterById;
        set
        {
            if (value == scanHeadFilterById)
            {
                return;
            }
            scanHeadFilterById = value;
            NotifyOfPropertyChange(() => ScanHeadFilterById);
            filteredView.Refresh();
            OnHeadSelectionChanged();
        }
    }

    public RawProfile? SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (Equals(value, selectedProfile))
            {
                return;
            }
            selectedProfile = value;
            NotifyOfPropertyChange(() => SelectedProfile);
        }
    }

    public double EncoderPulseInterval
    {
        get => encoderPulseInterval;
        set
        {
            if (value.Equals(encoderPulseInterval))
            {
                return;
            }
            encoderPulseInterval = value;
            NotifyOfPropertyChange(() => EncoderPulseInterval);
        }
    }

    public RelayCommand GoToFirstProfileCommand { get; }
    public RelayCommand GoToLastProfileCommand { get; }
    public RelayCommand GoToNextProfileCommand { get; }
    public RelayCommand GoToPreviousProfileCommand { get; }

    public DataManager(ILogger logger)
    {
        Logger = logger;
        BindingOperations.EnableCollectionSynchronization(SelectableHeads, _lock);
        filteredView = CollectionViewSource.GetDefaultView(Profiles);
        filteredView.Filter = Filter;
        GoToFirstProfileCommand = new RelayCommand((o) => GoToFirstProfile(), 
            (o) =>Profiles.Count > 0 && SelectedProfile != Profiles[0]);
        GoToLastProfileCommand = new RelayCommand((o) => GoToLastProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[^1]);
        GoToNextProfileCommand = new RelayCommand((o) => GoToNextProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[^1]);
        GoToPreviousProfileCommand = new RelayCommand((o) => GoToPreviousProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[0]);
    }

    

    private bool Filter(object obj)
    {
        if (ScanHeadFilterById == -1)
        {
            return true;
        }
        if (obj is RawProfile profile)
        {
            if (profile.ScanHeadId == (uint)ScanHeadFilterById)
            {
                return true;
            }
        }

        return false;
    }

    public void GoToFirstProfile()
    {
        if (scanHeadFilterById < 0)
        {
            SelectedProfile = Profiles[0];
        }
        else
        {
            SelectedProfile = Profiles.First(q => q.ScanHeadId == scanHeadFilterById);
        }
    }

    public void GoToLastProfile()
    {
        if (scanHeadFilterById < 0)
        {
            SelectedProfile = Profiles[^1];
        }
        else
        {
            SelectedProfile = Profiles.Last(q => q.ScanHeadId == scanHeadFilterById);
        }
    }

    public void GoToNextProfile()
    {

        if (scanHeadFilterById < 0)
        {
            // showing all heads, so just increase index
            SelectedProfile = Profiles[Profiles.IndexOf(SelectedProfile) + 1];
        }
        else
        {
            var idx = Profiles.IndexOf(SelectedProfile);
            var offset = 1;
            while (idx+offset < Profiles.Count && Profiles[idx+offset].ScanHeadId != scanHeadFilterById)
            {
                offset++;
            }
            SelectedProfile = Profiles[idx + offset];
        }
        
    }

    public void GoToPreviousProfile()
    {
        if (scanHeadFilterById < 0)
        {
            // showing all heads, so just decrease index
            SelectedProfile = Profiles[Profiles.IndexOf(SelectedProfile) - 1];
        }
        else
        {
            var idx = Profiles.IndexOf(SelectedProfile);
            var offset = -1;
            while (idx + offset >= 0 && Profiles[idx + offset].ScanHeadId != scanHeadFilterById)
            {
                offset--;
            }
            SelectedProfile = Profiles[idx + offset];
        }
    }
    
    public void FillAvailableHeads()
    {
        SelectableHeads.Clear();
        SelectableHeads.Add(new KeyValuePair<int, string>(-1,"All Heads"));
        foreach (uint headIds in Profiles.Select(q=>q.ScanHeadId).Distinct())
        {
            SelectableHeads.Add(new KeyValuePair<int, string>((int) headIds, $"Head {headIds}"));
        }
    }

    public void SetProfiles(List<RawProfile> newProfiles)
    {
        Profiles.Clear();
        foreach (var newProfile in newProfiles)
        {
            Profiles.Add(newProfile);
        }
        FillAvailableHeads();
        ReduceToStart();
        OnProfileDataAdded();
    }

    private void ReduceToStart()
    {
        foreach (var rawProfile in Profiles)
        {
            rawProfile.ReducedTimeStampNs = rawProfile.TimeStampNs - Profiles[0].TimeStampNs;
            rawProfile.ReducedEncoder = rawProfile.EncoderValue - Profiles[0].EncoderValue;
        }
    }

    protected virtual void OnProfileDataAdded()
    {
        ProfileDataAdded?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnHeadSelectionChanged()
    {
        HeadSelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}
