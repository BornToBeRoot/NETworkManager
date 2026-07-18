using MahApps.Metro.Controls;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Wake on LAN feature.
/// </summary>
public class WakeOnLANViewModel : ProfileHostViewModelBase
{
    #region Variables

    /// <summary>
    /// Gets or sets a value indicating whether the Wake on LAN operation is running.
    /// </summary>
    public bool IsRunning
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

    /// <summary>
    /// Gets the collection view for the MAC address history.
    /// </summary>
    public ICollectionView MACAddressHistoryView { get; }

    /// <summary>
    /// Gets or sets the MAC address to wake up.
    /// </summary>
    public string MACAddress
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

    /// <summary>
    /// Gets the collection view for the broadcast address history.
    /// </summary>
    public ICollectionView BroadcastHistoryView { get; }

    /// <summary>
    /// Gets or sets the broadcast address.
    /// </summary>
    public string Broadcast
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

    /// <summary>
    /// Gets or sets a value indicating whether the status message is displayed.
    /// </summary>
    public bool IsStatusMessageDisplayed
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

    /// <summary>
    /// Gets the status message to display.
    /// </summary>
    public string StatusMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the currently selected profile. Pre-fills <see cref="MACAddress"/>/<see cref="Broadcast"/>
    /// from the profile, unless a wake-up is currently running.
    /// </summary>
    public override ProfileInfo SelectedProfile
    {
        get => base.SelectedProfile;
        set
        {
            if (value == base.SelectedProfile)
                return;

            if (value != null && !IsRunning)
            {
                MACAddress = value.WakeOnLAN_MACAddress;
                Broadcast = value.WakeOnLAN_Broadcast;
            }

            base.SelectedProfile = value;
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="WakeOnLANViewModel"/> class.
    /// </summary>
    public WakeOnLANViewModel()
    {
        MACAddressHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.WakeOnLan_MACAddressHistory);
        BroadcastHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.WakeOnLan_BroadcastHistory);

        InitializeProfileHost();
    }

    #endregion

    #region Profile host

    protected override ApplicationName ApplicationName => ApplicationName.WakeOnLAN;

    protected override bool IsProfileEnabled(ProfileInfo profile) => profile.WakeOnLAN_Enabled;

    protected override string GetSearchableField(ProfileInfo profile) => profile.WakeOnLAN_MACAddress;

    #endregion

    #region ICommands & Actions

    /// <summary>
    /// Gets the command to wake up the target.
    /// </summary>
    public ICommand WakeUpCommand => new RelayCommand(_ => WakeUpAction(), WakeUpAction_CanExecute);

    private bool WakeUpAction_CanExecute(object parameter)
    {
        return Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen;
    }

    private void WakeUpAction()
    {
        var info = new WakeOnLANInfo
        {
            MagicPacket = WakeOnLAN.CreateMagicPacket(MACAddress),
            Broadcast = IPAddress.Parse(Broadcast),
            Port = SettingsManager.Current.WakeOnLAN_Port
        };

        AddMACAddressToHistory(MACAddress);
        AddBroadcastToHistory(Broadcast);

        _ = WakeUp(info);
    }

    /// <summary>
    /// Gets the command to wake up the selected profile.
    /// </summary>
    public ICommand WakeUpProfileCommand => new RelayCommand(_ => WakeUpProfileAction());

    private void WakeUpProfileAction()
    {
        _ = WakeUp(NETworkManager.Profiles.Application.WakeOnLAN.CreateInfo(SelectedProfile));
    }

    #endregion

    #region Methods

    private async Task WakeUp(WakeOnLANInfo info)
    {
        IsStatusMessageDisplayed = false;
        IsRunning = true;

        try
        {
            WakeOnLAN.Send(info);

            // Make the user happy, let him see a reload animation (and he cannot spam the reload command)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

            StatusMessage = Strings.MagicPacketSentMessage;
            IsStatusMessageDisplayed = true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }

        IsRunning = false;
    }

    private void AddMACAddressToHistory(string macAddress)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.WakeOnLan_MACAddressHistory.ToList(), macAddress,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.WakeOnLan_MACAddressHistory.Clear();
        OnPropertyChanged(nameof(MACAddress)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.WakeOnLan_MACAddressHistory.Add(x));
    }

    private void AddBroadcastToHistory(string broadcast)
    {
        // Create the new list
        var list = ListHelper.Modify(SettingsManager.Current.WakeOnLan_BroadcastHistory.ToList(), broadcast,
            SettingsManager.Current.General_HistoryListEntries);

        // Clear the old items
        SettingsManager.Current.WakeOnLan_BroadcastHistory.Clear();
        OnPropertyChanged(nameof(Broadcast)); // Raise property changed again, after the collection has been cleared

        // Fill with the new items
        list.ForEach(x => SettingsManager.Current.WakeOnLan_BroadcastHistory.Add(x));
    }

    #endregion
}
