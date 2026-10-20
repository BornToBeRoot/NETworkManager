using log4net;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Controls;
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

    private static readonly ILog Log = LogManager.GetLogger(typeof(FirewallViewModel));

    private readonly bool _isLoading;

    #region Rules

    /// <summary>
    /// Gets the loaded firewall rules.
    /// </summary>
    public ObservableRangeCollection<FirewallRule> Results { get; } = new();

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
    public bool IsAnySelectedEntryEnabled => GetSelectedRules().Any(rule => rule.IsEnabled);

    /// <summary>
    /// Indicates whether at least one of the selected rules is disabled (used to show "Enable").
    /// </summary>
    public bool IsAnySelectedEntryDisabled => GetSelectedRules().Any(rule => !rule.IsEnabled);

    /// <summary>
    /// Indicates whether at least one of the selected rules is managed by a policy (e.g. Group Policy or MDM)
    /// or created by Windows at runtime. Such rules can't be changed locally, so enable, disable and delete
    /// are blocked for the whole selection (also for mixed selections).
    /// The selection is only checked if the list contains such rules (see <see cref="_hasRulesManagedByPolicy"/>),
    /// because this is evaluated often (e.g. <see cref="ICommand.CanExecute"/>).
    /// </summary>
    public bool IsAnySelectedRuleManagedByPolicy => _hasRulesManagedByPolicy && GetSelectedRules().Any(rule => rule.IsManagedByPolicy);

    /// <summary>
    /// Indicates whether the loaded rules contain at least one rule managed by a policy. Updated on refresh.
    /// </summary>
    private bool _hasRulesManagedByPolicy;

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
    /// Gets whether all system firewall rules are shown or only rules managed by NETworkManager.
    /// Configured in the settings (see <see cref="SettingsInfo.Firewall_ShowAllRules"/>).
    /// </summary>
    public bool ShowAllRules => SettingsManager.Current.Firewall_ShowAllRules;

    /// <summary>
    /// Indicates whether the rules must be reloaded (e.g. because <see cref="ShowAllRules"/> was changed
    /// in the settings). Checked when the view becomes visible and after a running refresh.
    /// </summary>
    private bool _refreshPending;

    /// <summary>
    /// Indicates whether the view is currently visible.
    /// </summary>
    private bool _isViewVisible = true;

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
    /// Gets or sets whether the rules are loaded initially. The rule list is replaced by a loading indicator
    /// in this case. Other refreshes (e.g. after a rule was modified or the settings were changed) only show
    /// the status bar indicator (see <see cref="IsRefreshing"/>), so the list stays visible.
    /// </summary>
    public bool IsLoading
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

        // Reload the rules when the settings are changed (e.g. show all rules)
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

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
    /// Returns <see langword="true"/> when no refresh is currently running and no rule is being modified.
    /// </summary>
    private bool Refresh_CanExecute(object _) => !IsRefreshing && !IsModifying;

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

            await ModifyRulesAsync(() => Firewall.AddRuleAsync(BuildRule(instance)), "Error while adding firewall rule");
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
    /// Only executable when at least one selected rule is disabled, no selected rule is managed by a policy
    /// and modification is allowed.
    /// </summary>
    public ICommand EnableRuleCommand => new RelayCommand(parameter => { _ = SetRulesEnabled(true); },
        _ => ModifyRule_CanExecute() && IsAnySelectedEntryDisabled && !IsAnySelectedRuleManagedByPolicy);

    /// <summary>
    /// Gets the command to disable the selected firewall rule(s).
    /// Only executable when at least one selected rule is enabled, no selected rule is managed by a policy
    /// and modification is allowed.
    /// </summary>
    public ICommand DisableRuleCommand => new RelayCommand(parameter => { _ = SetRulesEnabled(false); },
        _ => ModifyRule_CanExecute() && IsAnySelectedEntryEnabled && !IsAnySelectedRuleManagedByPolicy);

    /// <summary>
    /// Enables or disables the selected rule(s) via PowerShell,
    /// then reloads the rule list to reflect the updated state.
    /// If at least one rule is not managed by NETworkManager, the user must confirm the change first.
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

        var notManagedWarning = GetNotManagedRulesWarning(rules);

        if (notManagedWarning != null)
        {
            var title = rules.Count == 1
                ? enabled ? Strings.EnableRule : Strings.DisableRule
                : string.Format(enabled ? Strings.EnableRulesX : Strings.DisableRulesX, rules.Count);

            var result = await DialogHelper.ShowConfirmationMessageAsync(
                Application.Current.MainWindow,
                title,
                notManagedWarning,
                ChildWindowIcon.Warn,
                enabled ? Strings.Enable : Strings.Disable);

            if (!result)
                return;
        }

        await ModifyRulesAsync(() => Firewall.SetRulesEnabledAsync(rules, enabled),
            $"Error while {(enabled ? "enabling" : "disabling")} firewall rule(s)");
    }

    /// <summary>
    /// Gets the command to open the dialog for editing the selected firewall rule.
    /// Only executable when exactly one rule managed by NETworkManager is selected and modification is allowed.
    /// Rules not managed by NETworkManager can't be edited (not supported yet), because they may use properties
    /// that can't be shown in the dialog (e.g. group, service or ICMP types). Rules with unsupported values
    /// (e.g. port keywords like RPC, see <see cref="FirewallRule.HasUnsupportedValues"/>) can't be edited either,
    /// because the dialog can't show or keep these values.
    /// </summary>
    public ICommand EditRuleCommand => new RelayCommand(parameter => { _ = EditRule(); },
        _ => ModifyRule_CanExecute() && IsSingleRuleSelected && SelectedResult.IsManaged && !SelectedResult.HasUnsupportedValues);

    /// <summary>
    /// Opens the edit-firewall-rule dialog pre-filled with the selected rule's properties.
    /// On confirmation, updates the changed properties of the rule via PowerShell (other properties and the
    /// internal ID are kept, see <see cref="Firewall.UpdateRuleAsync"/>),
    /// and refreshes the rule list.
    /// </summary>
    private async Task EditRule()
    {
        var childWindow = new FirewallRuleChildWindow();

        var childWindowViewModel = new FirewallRuleViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            await ModifyRulesAsync(() => Firewall.UpdateRuleAsync(instance.Entry, BuildRule(instance)),
                "Error while editing firewall rule");
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
    /// Only executable when exactly one rule is selected and modification is allowed.
    /// Rules with unsupported values (e.g. port keywords like RPC, see <see cref="FirewallRule.HasUnsupportedValues"/>)
    /// can't be copied, because the values would be lost and the copy would apply to more traffic (e.g. all ports).
    /// </summary>
    public ICommand CopyAsRuleCommand => new RelayCommand(parameter => { _ = CopyAsRule(); },
        _ => ModifyRule_CanExecute() && IsSingleRuleSelected && !SelectedResult.HasUnsupportedValues);

    /// <summary>
    /// Opens the rule dialog pre-filled with the selected rule's properties (name suffixed with " - Copy").
    /// On confirmation, creates a new rule via PowerShell (the selected rule is not changed)
    /// and refreshes the rule list.
    /// If the rule has properties that are not supported by NETworkManager and restrict the traffic
    /// (see <see cref="FirewallRule.HiddenProperties"/>), the user must first acknowledge that they are not copied.
    /// </summary>
    private async Task CopyAsRule()
    {
        var rule = SelectedResult;

        if (rule.HasHiddenProperties)
        {
            var result = await DialogHelper.ShowConfirmationMessageAsync(
                Application.Current.MainWindow,
                Strings.CopyRule,
                string.Format(Strings.CopyFirewallRuleHiddenPropertiesMessage, rule.Name,
                    GetHiddenPropertiesString(rule.HiddenProperties)),
                ChildWindowIcon.Warn,
                Strings.Continue);

            if (!result)
                return;
        }

        var childWindow = new FirewallRuleChildWindow();

        var childWindowViewModel = new FirewallRuleViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            await ModifyRulesAsync(() => Firewall.AddRuleAsync(BuildRule(instance)), "Error while copying firewall rule");
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, rule, EditMode.Copy);

        childWindow.Title = Strings.CopyRule;
        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Gets the command to permanently delete the selected firewall rule(s).
    /// Only executable when a rule is selected, no selected rule is managed by a policy and modification is allowed.
    /// </summary>
    public ICommand DeleteRuleCommand => new RelayCommand(parameter => { _ = DeleteRules(); },
        _ => ModifyRule_CanExecute() && SelectedResult != null && !IsAnySelectedRuleManagedByPolicy);

    /// <summary>
    /// Asks for confirmation and permanently deletes the selected rule(s) via PowerShell,
    /// then reloads the rule list. If at least one rule is not managed by NETworkManager, a warning
    /// is added to the confirmation. Any PowerShell error is written to the log and shown in the status bar.
    /// </summary>
    private async Task DeleteRules()
    {
        var rules = GetTargetEntries();

        if (rules.Count == 0)
            return;

        var message = rules.Count == 1
            ? string.Format(Strings.DeleteFirewallRuleMessage, DialogHelper.FormatItemList([rules[0].Name]))
            : string.Format(Strings.DeleteFirewallRulesMessage, DialogHelper.FormatItemList(rules.Select(rule => rule.Name)));

        var notManagedWarning = GetNotManagedRulesWarning(rules);

        if (notManagedWarning != null)
            message += Environment.NewLine + Environment.NewLine + notManagedWarning;

        var result = await DialogHelper.ShowConfirmationMessageAsync(
            Application.Current.MainWindow,
            rules.Count == 1 ? Strings.DeleteRule : string.Format(Strings.DeleteRulesX, rules.Count),
            message,
            notManagedWarning != null ? ChildWindowIcon.Warn : ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        await ModifyRulesAsync(() => Firewall.DeleteRulesAsync(rules), "Error while deleting firewall rule(s)");
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
               !IsRefreshing &&
               !IsModifying;
    }

    /// <summary>
    /// Gets the selected rules (the multi-selection, or the single selected rule) without copying them.
    /// Used for checks that are evaluated often (e.g. <see cref="ICommand.CanExecute"/>).
    /// </summary>
    private IEnumerable<FirewallRule> GetSelectedRules()
    {
        if (SelectedResults is { Count: > 1 })
            return SelectedResults.Cast<FirewallRule>();

        if (SelectedResult != null)
            return [SelectedResult];

        return [];
    }

    /// <summary>
    /// Indicates whether exactly one rule is selected (e.g. for edit or copy, which only work on a single rule).
    /// </summary>
    private bool IsSingleRuleSelected => SelectedResult != null && SelectedResults is not { Count: > 1 };

    /// <summary>
    /// Gets the rules to modify: the multi-selection, or the single selected rule.
    /// </summary>
    private List<FirewallRule> GetTargetEntries() => [.. GetSelectedRules()];

    /// <summary>
    /// Converts the hidden properties of a rule to a localized, bulleted list (e.g. "- ICMP types", "- Service").
    /// </summary>
    /// <param name="properties">The hidden properties.</param>
    private static string GetHiddenPropertiesString(FirewallRuleHiddenProperties properties)
    {
        var names = new List<string>();

        if (properties.HasFlag(FirewallRuleHiddenProperties.IcmpTypes))
            names.Add(Strings.IcmpTypes);

        if (properties.HasFlag(FirewallRuleHiddenProperties.Service))
            names.Add(Strings.Service);

        if (properties.HasFlag(FirewallRuleHiddenProperties.Package))
            names.Add(Strings.AppPackage);

        if (properties.HasFlag(FirewallRuleHiddenProperties.InterfaceAlias))
            names.Add(Strings.NetworkAdapters);

        if (properties.HasFlag(FirewallRuleHiddenProperties.Security))
            names.Add(Strings.UsersAndAuthentication);

        return DialogHelper.FormatItemList(names);
    }

    /// <summary>
    /// Builds a warning which lists the given rules that are not managed by NETworkManager
    /// (e.g. Windows built-in rules), because changing them may affect Windows or other applications.
    /// </summary>
    /// <param name="rules">The rules to check.</param>
    /// <returns>The warning, or <see langword="null"/> if all rules are managed by NETworkManager.</returns>
    private static string GetNotManagedRulesWarning(List<FirewallRule> rules)
    {
        var notManagedRules = rules.Where(rule => !rule.IsManaged).ToList();

        if (notManagedRules.Count == 0)
            return null;

        return string.Format(Strings.FirewallRulesNotManagedWarningMessage,
            DialogHelper.FormatItemList(notManagedRules.Select(rule => rule.Name)));
    }

    /// <summary>
    /// Runs the given modification and reloads the rule list afterwards, also if the modification
    /// failed (e.g. a rule was removed in the meantime), so the list always shows the actual state.
    /// Any error is written to the log and shown in the status bar.
    /// </summary>
    /// <param name="action">The modification to run.</param>
    /// <param name="errorLogMessage">Message written to the log if the modification fails.</param>
    private async Task ModifyRulesAsync(Func<Task> action, string errorLogMessage)
    {
        IsModifying = true;

        try
        {
            Exception error = null;

            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Log.Error(errorLogMessage, ex);
                error = ex;
            }

            var refreshed = await Refresh();

            // Show the error after the refresh, because the refresh overwrites the status message.
            // If the refresh failed too, keep its message, so it's visible that the list is not up to date.
            if (error != null)
            {
                var errorMessage = error is FirewallRuleNotFoundException
                    ? Strings.FirewallRulesNoLongerExistMessage
                    : error.Message;

                StatusMessage = refreshed ? errorMessage : errorMessage + Environment.NewLine + StatusMessage;
                IsStatusMessageDisplayed = true;
            }
        }
        finally
        {
            IsModifying = false;

            // The settings were changed while the rules were modified (see OnViewVisible)
            if (_refreshPending && _isViewVisible)
                _ = Refresh();
        }
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
    /// Loads the NETworkManager firewall rules (or all rules, if <see cref="ShowAllRules"/> is enabled)
    /// from the system via PowerShell and
    /// replaces the contents of <see cref="Results"/> with the new list.
    /// Updates <see cref="StatusMessage"/> throughout to reflect loading progress.
    /// </summary>
    /// <param name="init">
    /// When <see langword="true"/> the initial UI delay is skipped so the first load
    /// on startup feels immediate.
    /// </param>
    /// <returns><see langword="true"/> if the rules were loaded successfully; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> Refresh(bool init = false)
    {
        if (IsRefreshing)
            return false;

        IsRefreshing = true;

        // Show the loading indicator instead of the (empty) list on the initial load, see IsLoading
        IsLoading = init;

        StatusMessage = Strings.RefreshingDots;
        IsStatusMessageDisplayed = true;

        if (!init)
            await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        try
        {
            // Reset right before the setting is read, so only a change during the query triggers another refresh (see finally)
            _refreshPending = false;

            var rules = await Firewall.GetRulesAsync(onlyManagedRules: !ShowAllRules);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Replace all rules with a single notification, so the view is filtered / sorted only once
                // (can be 1000+ rules if all rules are shown). ResultsView.DeferRefresh() can't be used, because
                // the DataGrid accesses the view (selection / current item) while the items are changed.
                Results.ReplaceAll(rules);

                _hasRulesManagedByPolicy = rules.Any(rule => rule.IsManagedByPolicy);
            });

            StatusMessage = string.Format(Strings.ReloadedAtX, DateTime.Now.ToShortTimeString());
            IsStatusMessageDisplayed = true;

            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error while loading firewall rules", ex);

            StatusMessage = string.Format(Strings.FailedToLoadFirewallRulesMessage, ex.Message);
            IsStatusMessageDisplayed = true;

            return false;
        }
        finally
        {
            IsRefreshing = false;
            IsLoading = false;

            // The settings were changed during the refresh, reload with the new settings
            if (_refreshPending && _isViewVisible)
                _ = Refresh();
        }
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
        OnPropertyChanged(nameof(IsAnySelectedRuleManagedByPolicy));
    }

    /// <summary>
    /// Called when a setting is changed. Marks the rules to be reloaded if a setting changed that
    /// affects the rule list. The rules are reloaded when the view becomes visible again
    /// (the settings are opened in place of the view, see <see cref="OnViewVisible"/>).
    /// </summary>
    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.Firewall_ShowAllRules):
                OnPropertyChanged(nameof(ShowAllRules));

                _refreshPending = true;
                break;
        }
    }

    /// <summary>
    /// Called when the view becomes visible. Reloads the rules if a relevant setting was changed in the meantime.
    /// </summary>
    public void OnViewVisible()
    {
        _isViewVisible = true;

        // If rules are being modified, the refresh is done afterwards (see ModifyRulesAsync),
        // so the error message of the modification is not mixed up with the status of this refresh
        if (_refreshPending && !IsModifying)
            _ = Refresh();
    }

    /// <summary>
    /// Called when the view is hidden.
    /// </summary>
    public void OnViewHide()
    {
        _isViewVisible = false;
    }

    #endregion
}
