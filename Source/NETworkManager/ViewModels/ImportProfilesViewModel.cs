using System;
using System.Collections.Generic;
using System.Windows.Input;
using NETworkManager.Localization.Resources;
using NETworkManager.Profiles;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public sealed class ImportProfilesViewModel : ViewModelBase
{
    public ImportProfilesViewModel(Action<ImportProfilesViewModel> importCommand,
        Action<ImportProfilesViewModel> cancelHandler)
    {
        Methods = new List<ImportMethodItem>
        {
            new(ProfileImportSource.ActiveDirectory, Strings.ImportProfiles_Method_ActiveDirectory)
        };

        SelectedMethod = Methods[0];

        ImportCommand = new RelayCommand(_ => importCommand(this), _ => SelectedMethod != null);
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    public IReadOnlyList<ImportMethodItem> Methods { get; }

    public ImportMethodItem SelectedMethod
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICommand ImportCommand { get; }

    public ICommand CancelCommand { get; }

    public sealed record ImportMethodItem(ProfileImportSource Method, string DisplayName);
}
