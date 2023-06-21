﻿using Caliburn.Micro;
using RawViewer.Models;
using System.ComponentModel;

namespace RawViewer.ProfileDetail;

public class ProfileDetailViewModel : Screen
{
    private RawProfile? selectedProfile;
    public DataManager DataManager { get; }

    public double ZOffset =>
        selectedProfile != null ? selectedProfile.ReducedEncoder * DataManager.EncoderPulseInterval : 0;
    public double ZOffsetM => ZOffset/1000.0;

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
            NotifyOfPropertyChange(() => ZOffset);
            NotifyOfPropertyChange(() => ZOffsetM);
        }
    }

    public ProfileDetailViewModel(DataManager dataManager)
    {
        DataManager = dataManager;
        dataManager.PropertyChanged += SelectedProfileChanged;
    }

    private void SelectedProfileChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DataManager.SelectedProfile))
        {
            return;
        }

        SelectedProfile = DataManager.SelectedProfile;
    }
}
