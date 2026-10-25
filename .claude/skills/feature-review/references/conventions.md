# Conventions of the application

Patterns the application already follows. Use them as the baseline for consistency findings and for
proposed fixes. Each entry names a reference implementation. If the reviewed feature does it differently,
check whether the difference is intended before reporting it. If the reference itself has a weakness,
report that too.

## Contents

- View model basics
- Long-running actions (scan, lookup, capture)
- Refresh and modify (tools that change system state)
- Admin rights
- Data grids, selection and context menus
- Dialogs and confirmations
- Tooltips and status messages
- Layout
- Export and history
- Settings and profiles
- Navigation and run command
- PowerShell and external processes
- Strings, docs and changelog

## View model basics

- View models derive from `ViewModelBase` (`PropertyChangedBase` + `CopyDataToClipboardCommand`).
  Properties use the `field` keyword with `if (value == field) return; field = value; OnPropertyChanged();`.
- Commands: `public ICommand XCommand => new RelayCommand(_ => { _ = XAction(); }, X_CanExecute);` with an
  `async Task XAction()`. `async void` only for event handlers.
- `CanExecute` must be cheap: WPF re-evaluates it on every input. Don't copy collections or call services there.
- `OnViewVisible()` / `OnViewHide()` are called by the main window when switching tools. Auto-refresh timers
  stop on hide and restart on visible (reference: `ConnectionsViewModel`, `ListenersViewModel`, `WiFiViewModel`).

## Long-running actions (scan, lookup, capture)

Reference: `PortScannerViewModel`, `IPScannerViewModel`.

- `IsRunning` / `IsCanceling` flags, a `CancellationTokenSource` per run (dispose the old one), a Start/Cancel
  button that switches its text and icon, and `Enter` starts the action (start button with
  `IsDefault="{Binding IsRunning, Converter={StaticResource BooleanReverseConverter}}"`); inputs are
  read-only while running.
- Results arrive via model events and are added on the UI thread. Many small `Dispatcher.Invoke` calls per
  result are a performance smell for large scans.
- Status/error text below the input (`StatusMessage` + `IsStatusMessageDisplayed`).
- Inputs are added to a history (`AddHostToHistory` via `ListHelper.Modify`, limited by
  `GlobalStaticConfiguration.General_HistoryListEntries`).

## Refresh and modify (tools that change system state)

Reference: `HostsFileEditorViewModel`, `FirewallViewModel`.

- `IsRefreshing` and `IsModifying` flags. Refresh is blocked while modifying and modify actions are blocked
  while refreshing (`Refresh_CanExecute` / `Modify..._CanExecute`).
- Flags are reset in `finally` so an exception can't leave the UI disabled.
- After a change the list is reloaded, also after an error, and the error is shown after the reload
  (the reload overwrites the status message). Reference: `FirewallViewModel.ModifyRulesAsync`.
- Batch actions check that all selected items still exist before changing anything; items that are already
  in the target state are not rewritten when that would change formatting (hosts file).
- Refresh shows `RefreshingDots` → `ReloadedAtX`, with `GlobalStaticConfiguration.ApplicationUIRefreshInterval`
  delay for manual refreshes.
- Initial load: the grid is replaced by a centered `LoadingIndicator` (`LoadingIndicatorArcsStyle`) with a `*Dots`
  text below it (`IsLoading`, only for the first load). Later refreshes (after an action, F5, a changed setting)
  keep the list visible and only show the status bar indicator. Reference: `WiFiView.xaml`, `FirewallView.xaml`.
- Empty list: a `MessageTextBlock` in the grid row when the source collection is empty and the initial load is done
  (bind to the source collection's `Count`, not the filtered view, so an empty search result doesn't show it).
  Reference: `WiFiView.xaml` (`NoWiFiNetworksFound`), `FirewallView.xaml` (`NoFirewallRulesFoundMessage`).

## Admin rights

Reference: `HostsFileEditorView.xaml`, `FirewallView.xaml`, `NetworkInterfaceView.xaml`.

- Actions that need elevation are disabled via `CanExecute` (`ConfigurationManager.Current.IsAdmin`, or a
  feature-specific permission like `HasPermissionToConfigure`).
- Disabled actions get `wpfHelpers:PermissionToolTipHelper.Text` (the tool's `*AdminMessage`) and
  `PermissionToolTipHelper.HasPermission` — on buttons, context menu items and profile menu items.
- A banner at the bottom (`MahApps.Brushes.Gray10` background, `MessageTextBlock` with the `*AdminMessage`,
  **Restart as administrator** button with the `Shield` icon) is shown when not elevated.

## Data grids, selection and context menus

Reference: `HostsFileEditorView.xaml`, `FirewallView.xaml`.

- `controls:MultiSelectDataGrid` with `SelectedItem` → `SelectedResult` and `SelectedItemsList` → `SelectedResults`.
  `SelectedResults` is changed in place (no change notification): selection-dependent properties are raised
  in `OnContextMenuOpened()` called from the view's `ContextMenu_Opened`.
- Two context menus: `RowContextMenu` (one row) and `MultiRowContextMenu` (several rows), switched by a
  `DataTrigger` on `SelectedItems.Count`. `Opened="ContextMenu_Opened"` sets the menu's `DataContext`.
- Order: Enable / Disable (only the applicable one) → Edit... → Copy as... → Delete → separator → Copy
  (submenu, `CopyMenuItem` style) → Export...
- Icons (16×16 `Rectangle`, `MahApps.Brushes.Gray3`, `VisualBrush`): Enable `Material ToggleSwitchOutline`,
  Disable `Material ToggleSwitchOffOutline`, Edit `Modern Edit`, Copy as `Material ContentDuplicate`,
  Delete `Material Close`, Export `Material FileMoveOutline`, Refresh `Material Refresh`, Add `Material Plus`.
- Edit and Copy as work on a single item; Enable/Disable/Delete work on the selection (`GetTargetEntries()`).
  Exception: the Hosts File Editor has a bulk edit for the comment of several entries.
- Shortcuts: `F5` refresh, `F2` edit, `Del` delete; double-click a row = edit.
- Lists that are replaced completely on refresh (e.g. 1000+ firewall rules) use
  `ObservableRangeCollection<T>.ReplaceAll(items)` (`NETworkManager.Controls`): one `Reset` notification,
  so the filtered/sorted view is rebuilt only once. Don't wrap `Clear()`/`Add()` in
  `ICollectionView.DeferRefresh()` — the DataGrid accesses the view (selection / current item) while the
  items change, which throws "Cannot change or check the contents or Current position of CollectionView
  while Refresh is being deferred". Reference: `FirewallViewModel.Refresh`.
  Other users: `RunCommandViewModel.Suggestions`. Not yet used (same `Clear()` + `ForEach(Results.Add)` pattern on every
  refresh / auto refresh): `ConnectionsViewModel`, `ListenersViewModel`, `NeighborTableViewModel`,
  `HostsFileEditorViewModel` — report as a performance finding when reviewing them.
- Bindings on items that implement `INotifyPropertyChanged` (e.g. `IPScannerHostInfo`, view models via `BindingProxy`)
  use **plain paths** (`IsPingPending`, `PingInfo.Status`, `Data.ScanOptions.ResolveHostname`), not the typed form
  `(network:Type.Property)`. WPF registers a `PropertyChanged` listener with the property name the path parser stored;
  for a typed part that name stays `null` if the owner type can't be resolved early (templates, child bindings of a
  `MultiBinding`) → `ArgumentNullException` (`Parameter 'key'`) in `PropertyChangedEventManager.AddListener` as soon as
  the item is shown. The typed form is only safe on plain (non-notifying) items.
- Don't use a `Reset` for results that are *appended* while a scan runs (IP Scanner, Port Scanner, Traceroute): with a
  sorted view every `Reset` re-sorts the whole list and the grid loses its selection / scroll position. Those tools buffer
  results and flush them with a `DispatcherTimer` (reference: `IPScannerViewModel.FlushResultsBuffer`).

## Dialogs and confirmations

- Child windows: set `ConfigurationManager.Current.IsChildWindowOpen` before showing and reset it in both
  OK and Cancel handlers; dialog view models get OK/Cancel actions.
- Add/edit/copy dialogs share one view model with `EditMode` (`Add`, `Edit`, `Copy`; profiles use
  `ProfileEditMode`). Copy appends `" - " + Strings.CopyNoun` to a name where names identify items.
- Deletes are confirmed with `DialogHelper.ShowConfirmationMessageAsync(..., ChildWindowIcon.Info, Strings.Delete)`.
  Single item: `"Delete <item>"` title + the item in the message. Multiple: `"Delete <items> ({0})"` title +
  `DialogHelper.FormatItemList(...)` (first 10, then `(+N more)`).
- Lists in dialog messages are bulleted: `DialogHelper.FormatItemList` prefixes each item with `- `. Single items
  in a list-like message (e.g. "The selected rule is permanently deleted:") get the bullet too, so single and
  multiple selection look the same. Reference: `FirewallViewModel.DeleteRules`, `HostsFileEditorViewModel`.
- Menu items and buttons that open a dialog end with `...` (`*Dots` strings).

## Tooltips and status messages

- Long tooltip text uses `WrapToolTip` (max. 400px, wrapping); help/warn/error icons use `HelpToolTip`,
  `WarnToolTip`, `ErrorToolTip` (`ToolTipStyles.xaml`, `RectangleStyles.xaml`).
- Status text: `AccentTextBlock` / `StatusMessageTextBlock`, shown via `IsStatusMessageDisplayed`.

## Layout

- Tool view: `Grid Margin="10"`, 10px spacer rows/columns. Header row: `HeaderTextBlock` left, refresh
  (`CleanButton` with `ReloadAnimationHelper.IsReloading`) and search (`SearchTextBox`, width 250) right.
- Search box (`SearchTextBox`): filters while typing. Lists that can be large or slow to filter use a delay
  (`DispatcherTimer` with `GlobalStaticConfiguration.SearchDispatcherTimerTimeSpan`, reference: `FirewallViewModel`),
  short fixed lists filter on every key press. `Enter` only filters: `wpfHelpers:SearchTextBoxHelper.EnterCommand`
  (delayed search, e.g. `ApplySearchCommand` = stop the timer + `Refresh()`) or `SearchTextBoxHelper.HandleEnter="True"`
  (immediate search), so `Enter` never triggers the default button of the view / dialog / window. Exception: the
  profile panel search runs the selected profile on `Enter` (`ProfileExpanderPanel`).
- Bottom row: secondary actions left (e.g. Open hosts file, Windows Firewall settings), primary action right
  (e.g. Add entry...), `ImageWithTextButton`.
- Dialog: two-column grid (`1*`, 10, `2*`), 10px spacing between rows, OK (`HighlightedButton`, `IsDefault`)
  and Cancel (`IsCancel`) bottom right, focus on the first input in `ChildWindow_OnLoaded`.

## Export and history

- `ExportCommand` → `ExportChildWindow` with the supported file types (CSV/XML/JSON, sometimes TXT),
  "all" vs. "selected", last type/path stored in `<Tool>_ExportFileType` / `<Tool>_ExportFilePath`.
- Export logic in `ExportManager.<Tool>*.cs`.

## Settings and profiles

- Settings are properties `<Tool>_<Name>` in `SettingsInfo` with defaults in `GlobalStaticConfiguration`;
  renamed/removed settings are migrated in `SettingsManager`. Every setting shown in the UI must be used,
  and every behaviour-relevant constant that users might want to change should be considered as a setting.
- A tool that depends on a setting changed elsewhere (e.g. on its settings page) subscribes to
  `SettingsManager.Current.PropertyChanged`, marks the data as outdated and reloads it in `OnViewVisible()`
  (the settings are opened in place of the tool, so it is hidden while the setting changes). If a refresh or
  modification is running, reload once it has finished. Reference: `FirewallViewModel` (`Firewall_ShowAllRules`).
- Profile fields `<Tool>_*` in `ProfileInfo`; many can be overridden per group (`GroupInfo`), with fallback
  profile → group → settings. The same option should have the same label, default and validation in the
  settings page, the profile dialog and the group dialog.

## Navigation and run command

Reference: `NavigationManager`, `ApplicationRequest`, `IPScannerHostView.Open`, `RunCommandManager`.

- "Open in <other tool>" (context menus, buttons) calls `NavigationManager.Open(new ApplicationRequest(app) { Target = ... })`
  or a derived request with the tool's extra options (`PortScannerRequest.Ports`, `DNSLookupRequest`, `RDAPRequest`,
  `WakeOnLANRequest`, ...). Don't reference other view models or views directly.
- The main window switches to the tool and calls `IApplicationRequestHandler.Open(request)` on its view; host views
  turn it into `AddTab(target or profile, start: request.Start)`. `Start = false` only fills in the input (the user can
  adjust options first). A request for a hidden application shows an error.
- Every tool that accepts a target has a run command (`RunCommandManager.CreateCommands`: lowercase English command +
  aliases, translated syntax). The target is validated with the **same validator** as the tool's input field and the
  profile dialog (`ValidateTarget`); drift between these is a finding (see `.claude/reviews/run-command.md`).
- A new tool option that is useful from the keyboard can become a run command argument/type (and a property on a derived
  request); update the command table in `Website/docs/run-command.md` and `TargetApplications` (suggestions for a target
  without a command) when the accepted targets change.
- Values passed to another tool come from the result (e.g. hostname from DNS/NetBIOS). Pass a value the other tool can
  actually use (e.g. the IP address when the name is not resolvable by the app's DNS client).

## PowerShell and external processes

Reference: `Source/NETworkManager.Models/Firewall/Firewall.cs`.

- PowerShell runs on a shared runspace guarded by a `SemaphoreSlim`; one script per action, also for batches
  (pass arrays instead of one call per item).
- Every value inserted into a script is escaped (`PowerShellHelper.EscapeSingleQuotes`, `ToPsArray`),
  including IDs and names read from the system — the app may run elevated.
- Errors from `ps.Streams.Error` are turned into an exception with the joined messages and shown to the user.
- Read result properties with `result.Properties["X"]?.Value?.ToString()`, never `as string`: values can be
  wrapped in a `PSObject`, then `as string` returns `null` and the value is silently lost.
- Cmdlet parameters like `-Name` / `-DisplayName` of the `*-NetFirewallRule` cmdlets accept wildcards. IDs and names
  not created by the application (e.g. Windows rules with `[x64]` in the name) must be matched exactly
  (e.g. `Get-NetFirewallRule | Where-Object Name -eq ...`), otherwise a different or no item is changed.
- Custom commands (`CustomCommandInfo`, `PlaceholderHelper`, e.g. IP Scanner `{{IPAddress}}` / `{{Hostname}}`) insert
  values into a process command line. Values received from the network (DNS PTR, NetBIOS name, SNMP, HTTP) are
  untrusted: with `cmd.exe /c ...` characters like `&` or `|` run other commands. Only insert values that match the
  expected format (IP address, hostname regex).

## Strings, docs and changelog

- User-visible text comes from `Strings.resx` (English only; other languages via Transifex), added in
  alphabetical order to `Strings.Designer.cs`. Reuse existing strings (`Enable`, `Delete`, `CopyAsDots`,
  `PlusXMore`, ...) before adding new ones.
- `Website/docs/application/<tool>.md` documents toolbar, context menu, keyboard shortcuts and every dialog
  field (Type / Default / Example). Changes get an entry in `Website/docs/changelog/next-release.md` with a
  PR or commit link.
