using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models;
using NETworkManager.Models.Export;
using NETworkManager.Models.Firewall;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Firewall application.
/// </summary>
public class FirewallViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Maximum number of rules listed in the delete confirmation dialog.
    /// </summary>
    private const int DeleteRulesMaxDisplayed = 10;

    private static readonly ILog Log = LogManager.GetLogger(typeof(FirewallViewModel));

    private readonly bool _isLoading;

    #region Rules

    /// <summary>
    /// Gets the loaded firewall rules.
    /// </summary>
    public ObservableCollection<FirewallRule> Results { get; } = [];

    /// <summary>
    /// Gets the filtered/sorted view over <see cref="Results"/>.
    /// </summary>
    public ICollectionView ResultsView { get; }

    /// <summary>
    /// Gets or sets the currently selected firewall rule.
    /// </summary>
    public FirewallRule SelectedResult
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
    /// Gets or sets the list of selected firewall rules (multi-select).
    /// </summary>
    public IList SelectedResults
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    /// <summary>
    /// Indicates whether at least one of the selected rules is enabled (used to show "Disable").
    /// </summary>
    public bool IsAnySelectedEntryEnabled => GetTargetEntries().Any(rule => rule.IsEnabled);

    /// <summary>
    /// Indicates whether at least one of the selected rules is disabled (used to show "Enable").
    /// </summary>
    public bool IsAnySelectedEntryDisabled => GetTargetEntries().Any(rule => !rule.IsEnabled);

    /// <summary>
    /// Gets or sets the search text for filtering rules.
    /// </summary>
    public string RulesSearch
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            ResultsView.Refresh();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether a refresh is currently running.
    /// </summary>
    public bool IsRefreshing
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
    /// Gets or sets whether a rule is currently being modified (added, edited, enabled, disabled or deleted).
    /// </summary>
    public bool IsModifying
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
    /// Gets or sets whether the status message bar is shown.
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
    /// Gets or sets the status message text.
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

    #endregion

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="FirewallViewModel"/> class.
    /// </summary>
    public FirewallViewModel()
    {
        _isLoading = true;

        // Rules
        ResultsView = CollectionViewSource.GetDefaultView(Results);
        ResultsView.Filter = o =>
        {
            if (string.IsNullOrEmpty(RulesSearch))
                return true;

            if (o is not FirewallRule rule)
                return false;

            return rule.Name.IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   rule.Protocol.ToString().IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   rule.Action.ToString().IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1 ||
                   rule.Direction.ToString().IndexOf(RulesSearch, StringComparison.OrdinalIgnoreCase) > -1;
        };

        // Load firewall rules
        _ = Refresh(true);

        _isLoading = false;
    }

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to refresh the list of firewall rules from the system.
    /// Disabled while a refresh is already in progress.
    /// </summary>
    public ICommand RefreshCommand => new RelayCommand(parameter => { _ = RefreshAction(); }, Refresh_CanExecute);

    /// <summary>
    /// Returns <see langword="true"/> when no refresh is currently running.
    /// </summary>
    private bool Refresh_CanExecute(object _) => !IsRefreshing;

    /// <summary>
    /// Delegates to <see cref="Refresh"/> to reload the firewall rules.
    /// </summary>
    private async Task RefreshAction() => await Refresh();

    /// <summary>
    /// Gets the command to open the dialog for adding a new firewall rule.
    /// Only enabled when the application is running as administrator.
    /// </summary>
    public ICommand AddRuleCommand => new RelayCommand(parameter => { _ = AddRule(); }, _ => ModifyRule_CanExecute());

    /// <summary>
    /// Opens the add-firewall-rule dialog. On confirmation, creates the rule via PowerShell
    /// and refreshes the rule list.
    /// </summary>
    private async Task AddRule()
    {
        var childWindow = new FirewallRuleChildWindow();

        var childWindowViewModel = new FirewallRuleViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = true;

            try
            {
                await Firewall.AddRuleAsync(BuildRule(instance));
                await Refresh();
            }
            catch (Exception ex)
            {
                Log.Error("Error while adding firewall rule", ex);

                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }

            IsModifying = false;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddRule;
        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to enable the selected firewall rule(s).
    /// Only executable when at least one selected rule is disabled and modification is allowed.
    /// </summary>
    public ICommand EnableRuleCommand => new RelayCommand(parameter => { _ = SetRulesEnabled(true); }, _ => ModifyRule_CanExecute() && IsAnySelectedEntryDisabled);

    /// <summary>
    /// Gets the command to disable the selected firewall rule(s).
    /// Only executable when at least one selected rule is enabled and modification is allowed.
    /// </summary>
    public ICommand DisableRuleCommand => new RelayCommand(parameter => { _ = SetRulesEnabled(false); }, _ => ModifyRule_CanExecute() && IsAnySelectedEntryEnabled);

    /// <summary>
    /// Enables or disables the selected rule(s) via PowerShell (rules already in the target state are skipped),
    /// then reloads the rule list to reflect the updated state.
    /// Any PowerShell error is written to the log and shown in the status bar.
    /// </summary>
    /// <param name="enabled">
    /// <see langword="true"/> to enable the rules; <see langword="false"/> to disable them.
    /// </param>
    private async Task SetRulesEnabled(bool enabled)
    {
        var rules = GetTargetEntries();

        if (rules.Count == 0)
            return;

        IsModifying = true;

        try
        {
            await Firewall.SetRulesEnabledAsync(rules, enabled);
            await Refresh();
        }
        catch (Exception ex)
        {
            Log.Error($"Error while {(enabled ? "enabling" : "disabling")} firewall rule(s)", ex);

            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }

        IsModifying = false;
    }

    /// <summary>
    /// Gets the command to open the dialog for editing the selected firewall rule.
    /// Only executable when a rule is selected and modification is allowed.
    /// </summary>
    public ICommand EditRuleCommand => new RelayCommand(parameter => { _ = EditRule(); }, _ => ModifyRule_CanExecute() && SelectedResult != null);

    /// <summary>
    /// Opens the edit-firewall-rule dialog pre-filled with the selected rule's properties.
    /// On confirmation, deletes the old rule, creates the updated rule via PowerShell,
    /// and refreshes the rule list.
    /// </summary>
    private async Task EditRule()
    {
        var childWindow = new FirewallRuleChildWindow();

        var childWindowViewModel = new FirewallRuleViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = true;

            try
            {
                await Firewall.DeleteRuleAsync(instance.Entry);
                await Firewall.AddRuleAsync(BuildRule(instance));
                await Refresh();
            }
            catch (Exception ex)
            {
                Log.Error("Error while editing firewall rule", ex);

                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }

            IsModifying = false;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, SelectedResult, EditMode.Edit);

        childWindow.Title = Strings.EditRule;
        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to copy the selected firewall rule.
    /// Only enabled when the application is running as administrator.
    /// </summary>
    public ICommand CopyAsRuleCommand => new RelayCommand(parameter => { _ = CopyAsRule(); }, _ => ModifyRule_CanExecute() && SelectedResult != null);

    /// <summary>
    /// Opens the rule dialog pre-filled with the selected rule's properties (name suffixed with " - Copy").
    /// On confirmation, creates a new rule via PowerShell (the selected rule is not changed)
    /// and refreshes the rule list.
    /// </summary>
    private async Task CopyAsRule()
    {
        var childWindow = new FirewallRuleChildWindow();

        var childWindowViewModel = new FirewallRuleViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            IsModifying = true;

            try
            {
                await Firewall.AddRuleAsync(BuildRule(instance));
                await Refresh();
            }
            catch (Exception ex)
            {
                Log.Error("Error while copying firewall rule", ex);

                StatusMessage = ex.Message;
                IsStatusMessageDisplayed = true;
            }

            IsModifying = false;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, SelectedResult, EditMode.Copy);

        childWindow.Title = Strings.CopyRule;
        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to permanently delete the selected firewall rule(s).
    /// Only executable when a rule is selected and modification is allowed.
    /// </summary>
    public ICommand DeleteRuleCommand => new RelayCommand(parameter => { _ = DeleteRules(); }, _ => ModifyRule_CanExecute() && SelectedResult != null);

    /// <summary>
    /// Asks for confirmation and permanently deletes the selected rule(s) via PowerShell,
    /// then reloads the rule list. Any PowerShell error is written to the log and shown in the status bar.
    /// </summary>
    private async Task DeleteRules()
    {
        var rules = GetTargetEntries();

        if (rules.Count == 0)
            return;

        IsModifying = true;

        string message;

        if (rules.Count == 1)
        {
            message = string.Format(Strings.DeleteFirewallRuleMessage, rules[0].Name);
        }
        else
        {
            // Show the first rules, the rest is summarized as "(+X more)"
            var lines = rules.Take(DeleteRulesMaxDisplayed).Select(rule => rule.Name).ToList();

            if (rules.Count > DeleteRulesMaxDisplayed)
                lines.Add(string.Format(Strings.PlusXMore, rules.Count - DeleteRulesMaxDisplayed));

            message = string.Format(Strings.DeleteFirewallRulesMessage, string.Join(Environment.NewLine, lines));
        }

        var result = await DialogHelper.ShowConfirmationMessageAsync(
            Application.Current.MainWindow,
            rules.Count == 1 ? Strings.DeleteRule : string.Format(Strings.DeleteRulesX, rules.Count),
            message,
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
        {
            IsModifying = false;
            return;
        }

        try
        {
            await Firewall.DeleteRulesAsync(rules);
            await Refresh();
        }
        catch (Exception ex)
        {
            Log.Error("Error while deleting firewall rule(s)", ex);

            StatusMessage = ex.Message;
            IsStatusMessageDisplayed = true;
        }

        IsModifying = false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the application is running as administrator,
    /// no dialog is open, no child window is open and no rule is being modified — i.e. it is safe to modify a rule.
    /// </summary>
    private bool ModifyRule_CanExecute()
    {
        return ConfigurationManager.Current.IsAdmin &&
               Application.Current.MainWindow != null &&
               !((MetroWindow)Application.Current.MainWindow).IsAnyDialogOpen &&
               !ConfigurationManager.Current.IsChildWindowOpen &&
               !IsModifying;
    }

    /// <summary>
    /// Gets the rules to modify: the multi-selection, or the single selected rule.
    /// </summary>
    private List<FirewallRule> GetTargetEntries()
    {
        if (SelectedResults != null && SelectedResults.Count > 1)
            return [.. SelectedResults.Cast<FirewallRule>()];

        if (SelectedResult != null)
            return [SelectedResult];

        return [];
    }

    /// <summary>
    /// Gets the command to restart the application with administrator privileges.
    /// </summary>
    public ICommand RestartAsAdminCommand => new RelayCommand(parameter => { _ = RestartAsAdminAction(); });

    /// <summary>
    /// Restarts the application elevated. Shows an error dialog if the restart fails.
    /// </summary>
    private async Task RestartAsAdminAction()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.RestartApplication(true);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    /// <summary>
    /// Gets the command to export the current firewall rule list to a file.
    /// </summary>
    public ICommand ExportCommand => new RelayCommand(parameter => { _ = ExportAction(); });

    /// <summary>
    /// Opens the export child window and writes the selected or all firewall rules to the
    /// chosen file format (CSV, XML, or JSON). Shows an error dialog if the export fails.
    /// </summary>
    private Task ExportAction()
    {
        var childWindow = new ExportChildWindow();

        var childWindowViewModel = new ExportViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            try
            {
                ExportManager.Export(instance.FilePath, instance.FileType,
                    instance.ExportAll
                        ? Results
                        : new ObservableCollection<FirewallRule>(SelectedResults.Cast<FirewallRule>().ToArray()));
            }
            catch (Exception ex)
            {
                Log.Error("Error while exporting data as " + instance.FileType, ex);

                await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error,
                    Strings.AnErrorOccurredWhileExportingTheData + Environment.NewLine +
                    Environment.NewLine + ex.Message, ChildWindowIcon.Error);
            }

            SettingsManager.Current.Firewall_ExportFileType = instance.FileType;
            SettingsManager.Current.Firewall_ExportFilePath = instance.FilePath;
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, [
            ExportFileType.Csv, ExportFileType.Xml, ExportFileType.Json
        ], true, SettingsManager.Current.Firewall_ExportFileType,
        SettingsManager.Current.Firewall_ExportFilePath);

        childWindow.Title = Strings.Export;
        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #region Additional commands

    /// <summary>
    /// Gets the command to open the Windows Firewall management console (WF.msc).
    /// </summary>
    public ICommand OpenWindowsFirewallCommand => new RelayCommand(parameter => { _ = OpenWindowsFirewallAction(); });

    /// <summary>
    /// Action to open the Windows Firewall management console (WF.msc).
    /// Shows an error dialog if the process cannot be started.
    /// </summary>
    private async Task OpenWindowsFirewallAction()
    {
        try
        {
            ExternalProcessStarter.RunProcess("WF.msc");
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessageAsync(Application.Current.MainWindow, Strings.Error, ex.Message,
                ChildWindowIcon.Error);
        }
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// Loads all NETworkManager firewall rules from the system via PowerShell and
    /// replaces the contents of <see cref="Results"/> with the new list.
    /// Updates <see cref="StatusMessage"/> throughout to reflect loading progress.
    /// </summary>
    /// <param name="init">
    /// When <see langword="true"/> the initial UI delay is skipped so the first load
    /// on startup feels immediate.
    /// </param>
    private async Task Refresh(bool init = false)
    {
        if (IsRefreshing)
            return;

        IsRefreshing = true;
        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        if (!init)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        try
        {
            var rules = await Firewall.GetRulesAsync();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Results.Clear();

                foreach (var rule in rules)
                    Results.Add(rule);
            });

            StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
            IsStatusMessageDisplayed = true;
        }
        catch (Exception ex)
        {
            Log.Error("Error while loading firewall rules", ex);

            StatusMessage = string.Format(Strings.FailedToLoadFirewallRulesMessage, ex.Message);
            IsStatusMessageDisplayed = true;
        }

        IsRefreshing = false;
    }

    /// <summary>
    /// Builds a <see cref="FirewallRule"/> from the values the user entered in the dialog.
    /// </summary>
    /// <param name="vm">The dialog ViewModel containing the user's input.</param>
    private static FirewallRule BuildRule(FirewallRuleViewModel vm) => new()
    {
        Name = vm.Name,
        IsEnabled = vm.IsEnabled,
        Description = vm.Description ?? string.Empty,
        Direction = vm.Direction,
        Action = vm.Action,
        Protocol = vm.Protocol,
        LocalPorts = ParsePortsString(vm.LocalPorts),
        RemotePorts = ParsePortsString(vm.RemotePorts),
        LocalAddresses = ParseAddressesString(vm.LocalAddresses),
        RemoteAddresses = ParseAddressesString(vm.RemoteAddresses),
        Program = string.IsNullOrWhiteSpace(vm.Program) ? null : new FirewallRuleProgram(vm.Program),
        InterfaceType = vm.InterfaceType,
        NetworkProfiles = [vm.NetworkProfileDomain, vm.NetworkProfilePrivate, vm.NetworkProfilePublic]
    };

    /// <summary>
    /// Parses a semicolon-separated port string (e.g. <c>"80; 443; 8080-8090"</c>) into a
    /// list of <see cref="FirewallPortSpecification"/> objects.
    /// </summary>
    /// <param name="value">The semicolon-separated port string from the dialog.</param>
    private static List<FirewallPortSpecification> ParsePortsString(string value)
    {
        var list = new List<FirewallPortSpecification>();

        if (string.IsNullOrWhiteSpace(value))
            return list;

        foreach (var token in value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var dash = token.IndexOf('-');

            if (dash > 0 &&
                int.TryParse(token[..dash], out var start) &&
                int.TryParse(token[(dash + 1)..], out var end))
            {
                list.Add(new FirewallPortSpecification(start, end));
            }
            else if (int.TryParse(token, out var port))
            {
                list.Add(new FirewallPortSpecification(port));
            }
        }

        return list;
    }

    /// <summary>
    /// Parses a semicolon-separated address string (e.g. <c>"192.168.1.0/24; LocalSubnet"</c>)
    /// into a list of address strings.
    /// </summary>
    /// <param name="value">The semicolon-separated address string from the dialog.</param>
    private static List<string> ParseAddressesString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return [.. value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    /// <summary>
    /// Called when a context menu is opened. Updates the selection dependent properties,
    /// because <see cref="SelectedResults"/> is changed in place (no change notification).
    /// </summary>
    public void OnContextMenuOpened()
    {
        OnPropertyChanged(nameof(IsAnySelectedEntryEnabled));
        OnPropertyChanged(nameof(IsAnySelectedEntryDisabled));
    }

    /// <summary>
    /// Called when the view becomes visible.
    /// </summary>
    public void OnViewVisible()
    {
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
    }

    #endregion
}
