using Caliburn.Micro;
using JoeScan.LogScanner.Core.Config;
using JoeScan.LogScanner.Core.Interfaces;
using JoeScan.LogScanner.Core.Models;
using NLog;
using RawViewer.Helpers;
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

namespace RawViewer.Models;

public class DataManager : PropertyChangedBase
{
    #region Events

    public event EventHandler ProfileDataAdded;
    public event EventHandler HeadSelectionChanged;
    public event EventHandler CameraSelectionChanged;

    #endregion

    #region Private Fields

    private RawProfile? selectedProfile;
    private List<Profile> originalData;
    private readonly object locker = new();
    private readonly ICollectionView filteredView;

    private int scanHeadFilterById = -1;
    private int scanHeadFilterByCamera = 0;
    private double encoderPulseInterval;
    private bool useFlightsAndWindowFilter;

    #endregion

    #region Injected Properties

    public ILogger Logger { get; }
    public ILogAssembler Assembler { get; }
    public IFlightsAndWindowFilter FlightsFilter { get; }

    #endregion

    #region UI Bound Properties

    public IObservableCollection<RawProfile> Profiles { get; } = new BindableCollection<RawProfile>();


    public ObservableCollection<KeyValuePair<int, string>> SelectableHeads { get; } =
        new ObservableCollection<KeyValuePair<int, string>>();
    public ObservableCollection<KeyValuePair<int, string>> SelectableCameras { get; } =
        new ObservableCollection<KeyValuePair<int, string>>()
        {
            new KeyValuePair<int, string>(0, "A/B"),
            new KeyValuePair<int, string>(1, "A"),
            new KeyValuePair<int, string>(2, "B"),
        };

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
    public int ScanHeadFilterByCamera
    {
        get => scanHeadFilterByCamera;
        set
        {
            if (value == scanHeadFilterByCamera)
            {
                return;
            }
            scanHeadFilterByCamera = value;
            NotifyOfPropertyChange(() => ScanHeadFilterByCamera);
            filteredView.Refresh();
            OnCameraSelectionChanged();
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

    public bool UseFlightsAndWindowFilter
    {
        get => useFlightsAndWindowFilter;
        set
        {
            if (value == useFlightsAndWindowFilter)
            {
                return;
            }
            useFlightsAndWindowFilter = value;
            FilterAndAdd();
            NotifyOfPropertyChange(() => UseFlightsAndWindowFilter);
        }
    }

    #endregion


    #region Commands

    public RelayCommand GoToFirstProfileCommand { get; }
    public RelayCommand GoToLastProfileCommand { get; }
    public RelayCommand GoToNextProfileCommand { get; }
    public RelayCommand GoToPreviousProfileCommand { get; }

    #endregion

    #region Lifecycle

    public DataManager(ILogger logger, CoreConfig config, ILogAssembler assembler,
        IFlightsAndWindowFilter flightsFilter)
    {
        Logger = logger;
        Assembler = assembler;
        FlightsFilter = flightsFilter;
        BindingOperations.EnableCollectionSynchronization(SelectableHeads, locker);
        filteredView = CollectionViewSource.GetDefaultView(Profiles);
        filteredView.Filter = Filter;
        GoToFirstProfileCommand = new RelayCommand((o) => GoToFirstProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[0]);
        GoToLastProfileCommand = new RelayCommand((o) => GoToLastProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[^1]);
        GoToNextProfileCommand = new RelayCommand((o) => GoToNextProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[^1]);
        GoToPreviousProfileCommand = new RelayCommand((o) => GoToPreviousProfile(),
            (o) => Profiles.Count > 0 && SelectedProfile != Profiles[0]);
        EncoderPulseInterval = config.EncoderPulseInterval;
    }

    #endregion


    #region Public Methods

    public void SetProfiles(List<Profile> newProfiles)
    {  
        originalData = newProfiles;
        FilterAndAdd();
    }

    #endregion

    #region Private Methods

    private bool Filter(object obj)
    {
        if (ScanHeadFilterById == -1 && ScanHeadFilterByCamera == 0)
        {
            return true;
        }
        if (obj is RawProfile profile)
        {

            if (ScanHeadFilterById != -1 && profile.ScanHeadId != (uint)ScanHeadFilterById)
            {
                return false;
            }

            if (ScanHeadFilterByCamera != 0 && profile.Camera != (uint)ScanHeadFilterByCamera)
            {
                return false;
            }

            return true;
        }

        return false;
    }

    private void FilterAndAdd()
    {
        Profiles.Clear();
        SelectableHeads.Clear();
        SelectableHeads.Add(new KeyValuePair<int, string>(-1, "*"));
        HashSet<uint> headsInFile = new HashSet<uint>();
        int index = 0;
        foreach (var p in originalData)
        {
            var rp = UseFlightsAndWindowFilter ? new RawProfile(FlightsFilter.Apply((Profile)p.Clone())) { Index = index++ } : new RawProfile(p) { Index = index++ };
            Profiles.Add(rp);
            rp.ReducedTimeStampNs = rp.TimeStampNs - Profiles[0].TimeStampNs;
            rp.ReducedEncoder = rp.EncoderValue - Profiles[0].EncoderValue;
            headsInFile.Add(p.ScanHeadId);
        }
        foreach (uint headIds in headsInFile)
        {
            SelectableHeads.Add(new KeyValuePair<int, string>((int)headIds, $"{headIds}"));
        }
        OnProfileDataAdded();
    }

    #endregion


    #region Event Invocation

    protected virtual void OnProfileDataAdded()
    {
        ProfileDataAdded?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnHeadSelectionChanged()
    {
        HeadSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnCameraSelectionChanged()
    {
        CameraSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region UI Callbacks

    public void RunAssembler()
    {
        foreach (var rawProfile in Profiles)
        {
            Assembler.AddProfile(rawProfile.Profile);
        }
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
            while (idx + offset < Profiles.Count && Profiles[idx + offset].ScanHeadId != scanHeadFilterById)
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

    #endregion
}
