# Where a feature lives

Use this to find all files of a feature before reviewing it. Paths are relative to the repository root.
Start with `git grep -l "<ApplicationName>" -- Source Website` (e.g. `PortScanner`, `HostsFileEditor`) and
sort the hits into the categories below. Some names differ between places (e.g. `WakeOnLAN` / `WakeOnLan`,
`ARPTable` / `NeighborTable`), so also grep for the visible name and the docs file name.

## Contents

- Touch points of a tool
- Tool shapes (host view vs. single view)
- Review units

## Touch points of a tool

| Area | Location | What to check there |
|---|---|---|
| Registration | `Source/NETworkManager.Models/ApplicationName.cs`, `ApplicationManager.cs` (name, icon, default visibility) | Icon matches the one used elsewhere for the tool |
| Main window | `Source/NETworkManager/MainWindow.xaml.cs` — view creation (`case ApplicationName.X`), `OnViewHide`/`OnViewVisible`, `AddTab(...)` for "open in other tool" | View lifecycle: what keeps running when hidden |
| Business logic | `Source/NETworkManager.Models/<Tool>/` or `Source/NETworkManager.Models/Network/<Tool>*.cs` (+ `*Options`, `*Info`, `*Args`) | Bugs, performance, cancellation, events |
| View model | `Source/NETworkManager/ViewModels/<Tool>ViewModel.cs`, `<Tool>HostViewModel.cs` (tabs + profiles), `<Tool>SettingsViewModel.cs`, dialog view models | Commands, `CanExecute`, state flags, threading |
| View | `Source/NETworkManager/Views/<Tool>View.xaml(.cs)`, `<Tool>HostView.xaml`, `<Tool>SettingsView.xaml`, `*ChildWindow.xaml` | Layout, context menus, bindings, shortcuts |
| Controls | `Source/NETworkManager/Controls/` (remote connection controls, charts, map) | Embedded processes, resize, focus, disposal |
| Settings | `Source/NETworkManager.Settings/SettingsInfo.cs` (`<Tool>_*` properties), `GlobalStaticConfiguration.cs` (defaults), `SettingsManager.cs` (upgrade/migration), `SettingsName.cs`, `SettingsViewManager.cs` | Defaults, migration of old values, every setting actually used |
| Runtime state | `Source/NETworkManager.Settings/ConfigurationInfo.cs` (e.g. `<Tool>TabCount`, `IsAdmin`, `IsChildWindowOpen`) | |
| Profiles | `Source/NETworkManager.Profiles/ProfileInfo.cs` (`<Tool>_*` fields), `ProfileName.cs`, `ProfileViewManager.cs`, `Application/<Tool>.cs` (build session info from profile + group), `GroupInfo.cs` / `GroupViewName.cs` (group-level settings) | Profile field ↔ group inheritance ↔ settings fallback |
| Profile UI | `Source/NETworkManager/ViewModels/ProfileViewModel.cs`, `Views/ProfileChildWindow.xaml` (tool tab), `Views/GroupChildWindow.xaml`, `ProfileDialogManager.cs` | Same fields, labels and validation in profile, group and settings |
| Export | `Source/NETworkManager.Models/Export/ExportManager.<Tool>*.cs`, `ExportChildWindow` | Same columns/formats as similar tools |
| Run command | `Source/NETworkManager/RunCommandManager.cs` | Arguments/placeholder accepted |
| Documentation | `Source/NETworkManager.Documentation/DocumentationManager.cs` (help links), `Website/docs/application/<tool>.md`, `Website/docs/settings/*.md`, `Website/docs/commandline-arguments.md` | Docs match behaviour, defaults and shortcuts |
| Strings | `Source/NETworkManager.Localization/Resources/Strings.resx` (+ `Strings.Designer.cs`) | Only `Strings.resx` (English) is edited; other languages come from Transifex |
| Styles | `Source/NETworkManager/Resources/Styles/*.xaml` (buttons, text blocks, tooltips, rectangles) | Shared styles used instead of local ones |
| Helpers | `Source/NETworkManager.Utilities/`, `Source/NETworkManager.Utilities.WPF/`, `Source/NETworkManager.Converters/`, `Source/NETworkManager.Validators/`, `Source/NETworkManager/DialogHelper.cs` | Existing helper not reused / duplicated logic |

## Tool shapes

- **Host view with tabs + profiles** (`<Tool>HostView` + `<Tool>HostViewModel`, Dragablz tabs, profile panel):
  DNS Lookup, IP Geolocation, IP Scanner, Lookup, Ping Monitor, Port Scanner, PowerShell, PuTTY, RDAP,
  Remote Desktop, SNMP, SNTP Lookup, Subnet Calculator, TigerVNC, Traceroute, Web Console, Whois.
  Compare tab handling, profile context menu ("extra items"), tab title, "open in other tool".
- **Single view** (`<Tool>View` only): Dashboard, Network Interface (with profiles), WiFi, Hosts File Editor,
  Firewall, Discovery Protocol, Wake on LAN (with profiles), Bit Calculator, Connections, Listeners,
  Neighbor Table. Compare refresh logic, context menus, admin handling, export.
- **Removed but still referenced**: `AWSSessionManager` (hidden in `ApplicationManager`, kept for settings
  migration, docs page marked as removed) — not a finding unless something still uses it.

## Review units

Review one unit at a time. Tools:

- **Network**: Dashboard (+ widgets: Network Connection, IP API, Speed Test), Network Interface, WiFi, IP Scanner,
  Port Scanner, Ping Monitor, Traceroute, DNS Lookup, Discovery Protocol, Wake on LAN, SNMP, SNTP Lookup
- **Remote access**: Remote Desktop, PowerShell, PuTTY, TigerVNC, Web Console
- **System**: Hosts File Editor, Firewall, Connections, Listeners, Neighbor Table
- **Lookup / calculators**: Whois, RDAP, IP Geolocation, Lookup (OUI/port), Subnet Calculator, Bit Calculator

Settings pages (`Source/NETworkManager/Views/Settings*View.xaml` + per-tool `<Tool>SettingsView.xaml`):
General, Window, Appearance, Language, Network, Status, Hotkeys, Autostart, Update, Settings (location,
import/export, reset), Profiles, and each tool's settings page (review together with the tool, or all tool
settings pages together for consistency).

Cross-cutting areas:

- **Profiles and groups**: `NETworkManager.Profiles`, `ProfilesView`, `ProfileChildWindow`, `GroupChildWindow`,
  profile files, encryption (`Credentials*ChildWindow`), import (CSV/AD), filter/tags, profile panel in host views
- **Main window and navigation**: `MainWindow.xaml(.cs)`, application list, run command (`CommandLineWindow`,
  `RunCommandManager`), tray, hotkeys, `StatusWindow`, `NotificationWindow`, welcome/upgrade dialogs
- **Export and dialogs**: `ExportManager`, `ExportChildWindow`, `DialogHelper`, message/confirmation child windows
- **Settings infrastructure**: `SettingsManager` (load/save/upgrade), `LocalSettingsManager`, policies
  (`PolicyManager`, `config.json.example`: settings managed by administrator)
