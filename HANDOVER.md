# Handover: Global DNS suffix + custom DNS server rework

**Status:** Implemented, uncommitted, NOT built/tested (no Windows/.NET SDK in the sandbox that did this work).
**Branch:** `feature/3559` (clean branch off `main` @ `38989cb22`, no commits yet — all changes below are working-tree edits only).
**This file is untracked** (not added to git) — delete it once you're done, or `git add`/commit it if you want it kept.

## What was asked

Extend the global Network settings (`SettingsNetworkViewModel`/`SettingsNetworkView`) to add two things that already exist in the DNS Lookup tool's own settings, making them apply to *all* tools that resolve hostnames (Ping, Traceroute, PortScanner, IPScanner, Connection, NTP Lookup, etc.), not just DNS Lookup:

1. **DNS suffix support** — "Add DNS suffix (primary) to hostname" + "Use custom DNS suffix" + the suffix textbox.
2. **Rework of the custom DNS server input** — was a single semicolon-separated IP-only textbox; wanted something closer to DNS Lookup's server dialog (label + Edit button instead of a raw textbox).

## Research findings (still true, useful context)

- There are **two independent DNS resolution paths** in the codebase:
  - The shared singleton `NETworkManager.Utilities.DNSClient`, configured once from `MainWindow.ConfigureDNSServer()` off the `Network_*` settings. This is the resolver behind PTR (reverse) lookups in Ping/Traceroute/PortScanner/IPScanner/Connection/NetworkConnectionWidget, **and** the forward A/AAAA chokepoint `DNSClientHelper.ResolveAorAaaaAsync` used by `HostRangeHelper` (host-range parsing shared by Ping/Traceroute/PortScanner/IPScanner) and `SNTPLookup`. This is the one place that needed suffix support to make it "global."
  - DNS Lookup's own independent path in `NETworkManager.Models.Network.DNSLookup.cs`, which builds a fresh `LookupClient` per query and never touches the shared singleton. **Not touched by this change** — it already had its own suffix logic (`DNSLookup_AddDNSSuffix`/`DNSLookup_UseCustomDNSSuffix`/`DNSLookup_CustomDNSSuffix`), which was the template copied for the global settings.
- DNS Lookup's server-editing dialog (`ServerConnectionInfoProfileChildWindow` + `ServerConnectionInfoProfileViewModel`) is a **named-profile** editor (Name + list of Server:Port). Global settings only need one unnamed list, so it's reused with a new `isNameReadOnly` flag rather than building a new dialog.
- `DNSClient.Configure()` calls `IPAddress.Parse(server)` directly on custom DNS server entries — **custom DNS servers must stay IP-only** (hostnames would throw). DNS Lookup's dialog allows hostnames for its own servers; ours is opened with `allowOnlyIPAddress: true`.

## Decisions made (confirmed with user via AskUserQuestion)

1. **"Add DNS suffix" defaults to ON** (matches DNS Lookup's own default), even though this changes hostname-resolution behavior for existing users across Ping/Traceroute/PortScanner/IPScanner/SNTP after upgrade.
2. **Custom DNS server editing reuses the DNS Lookup profile dialog** (`ServerConnectionInfoProfileChildWindow`/`ServerConnectionInfoProfileViewModel`), opened with the profile name fixed to `Strings.DNSServers` ("DNS server(s)") and **read-only** (new `IsNameReadOnly` property/binding), since there's only one global list, not multiple named profiles.
3. **Per-server port is configurable** (previously hardcoded to 53). DnsClient.net already supports arbitrary `IPEndPoint` per server, so this was low cost.

## Files changed (all uncommitted)

| File | Change |
|---|---|
| `Source/NETworkManager.Settings/GlobalStaticConfiguration.cs` | Added `Network_AddDNSSuffix => true` default. **Note:** a linter/format pass (not me) reordered this line to sit alphabetically above `Network_ResolveHostnamePreferIPv4` — that reorder is intentional, don't revert it. |
| `Source/NETworkManager.Settings/SettingsInfo.cs` | Added `Network_CustomDNSServers` (`ObservableCollection<ServerConnectionInfo>`), `Network_AddDNSSuffix`, `Network_UseCustomDNSSuffix`, `Network_CustomDNSSuffix`. Marked old `Network_CustomDNSServer` (string) `[Obsolete]` — kept only for the migration below. |
| `Source/NETworkManager.Settings/SettingsManager.cs` | New upgrade step `UpgradeTo_2026_8_17_0()` (registered in the `Upgrade()` dispatcher chain) migrates the old semicolon-separated `Network_CustomDNSServer` string into `Network_CustomDNSServers` at port 53/UDP, preserving current behavior for upgraders. **Version number `2026.8.17.0` was a placeholder guess (today's date) — confirm/adjust to match whatever version this actually ships in**, following the project's `yyyy.M.d.0` date-based versioning (see `AGENTS.md`). |
| `Source/NETworkManager.Utilities/DNSClientSettings.cs` | Added `AddDNSSuffix` (bool) and `DNSSuffix` (string, already-resolved value) fields. |
| `Source/NETworkManager.Utilities/DNSClient.cs` | `Configure()` precomputes `_addSuffix`. New private `AddDNSSuffixIfConfigured(query)` appends `.{suffix}` to hostnames without a dot; called from `ResolveAAsync`/`ResolveAaaaAsync` only (not PTR/CNAME — matches `DNSLookup.cs`'s behavior of skipping suffix for reverse lookups). |
| `Source/NETworkManager/MainWindow.xaml.cs` | `ConfigureDNSServer()` rewritten to build the server list from `Network_CustomDNSServers` (real per-entry ports) and to populate `AddDNSSuffix`/`DNSSuffix` on `DNSClientSettings` (custom suffix trimmed of leading `.`, else `IPGlobalProperties.GetIPGlobalProperties().DomainName`, mirroring `DNSLookupViewModel.QueryAsync()`). `SettingsManager_PropertyChanged` switch extended with cases for `Network_CustomDNSServers`, `Network_AddDNSSuffix`, `Network_UseCustomDNSSuffix`, `Network_CustomDNSSuffix` so live settings changes reconfigure DNS immediately (same as the pre-existing two cases). |
| `Source/NETworkManager/ViewModels/ServerConnectionInfoProfileViewModel.cs` | New optional ctor param `isNameReadOnly = false` → new bindable `IsNameReadOnly` property. |
| `Source/NETworkManager/Views/ServerConnectionInfoProfileChildWindow.xaml` | `TextBoxName.IsReadOnly` bound to `IsNameReadOnly`. |
| `Source/NETworkManager/Views/ServerConnectionInfoProfileChildWindow.xaml.cs` | `ChildWindow_OnLoaded` now focuses `TextBoxServer` instead of `TextBoxName` when the name is read-only (small UX polish so tab focus doesn't land on an uneditable field). |
| `Source/NETworkManager/ViewModels/SettingsNetworkViewModel.cs` | Rewritten. New: `CustomDNSServersDisplay` (read-only joined-string of `Network_CustomDNSServers`), `AddDNSSuffix`/`UseCustomDNSSuffix`/`CustomDNSSuffix` properties (copied pattern from `DNSLookupSettingsViewModel`), `EditCustomDNSServersCommand` → `EditCustomDNSServers()` which opens the reused profile dialog (see decisions above) and writes the result back to `Network_CustomDNSServers`. Old `CustomDNSServer` string property removed entirely. |
| `Source/NETworkManager/Views/SettingsNetworkView.xaml` | Old DNS server `TextBox` replaced with a label (`CustomDNSServersDisplay`) + Edit icon button (`EditCustomDNSServersCommand`, `iconPacks:Modern Kind=Edit`, tooltip `Strings.EditDNSServer`). Added suffix toggle/textbox section copied verbatim in structure from `DNSLookupSettingsView.xaml` (reuses existing localized strings — no new resx keys needed anywhere in this change). |
| `Source/NETworkManager.Validators/MultipleIPAddressesValidator.cs` | **Deleted** — was only referenced by the old DNS server textbox, now dead code. Confirmed via repo-wide grep before deleting. |
| `Website/docs/settings/network.md` | Updated to document the new "DNS server(s)" editing UX (now IP+port, edited via dialog) and the three new suffix settings. |

## Verification status — IMPORTANT, not done yet

**Nothing has been compiled or run.** This sandbox has no `dotnet` SDK and the project targets `net10.0-windows10.0.22621.0` (WPF, Windows-only), so it can't be built here at all. Verification so far was manual code tracing only:
- Confirmed no other files reference the deleted validator or the removed `CustomDNSServer` VM property (repo-wide grep).
- Confirmed `ServerConnectionInfo`/`TransportProtocol`/`ObservableCollection<ServerConnectionInfo>` patterns already exist and serialize fine elsewhere (`DNSLookup_DNSServers` uses the same shapes) — System.Text.Json settings persistence should just work.
- Confirmed `ServerValidator` genuinely enforces IP-only when `AllowOnlyIPAddress=true` (checked the validator source).
- Confirmed `DNSClientSettings` is only constructed in one place (`MainWindow.ConfigureDNSServer()`), so no other caller needed updating.

**Next session should, in this order:**
1. **Build on Windows** (`dotnet build` or open in VS) and fix any compile errors — I could not verify this.
2. Manually test in the running app:
   - Toggle "Use custom DNS server" → Edit button opens dialog with fixed read-only "DNS server(s)" name → add/edit/remove IP:port entries → Save → label updates, `Network_CustomDNSServers` persists across restart.
   - Verify hostname-only entries are rejected in that dialog (IP-only enforcement).
   - Toggle "Add DNS suffix (primary) to hostname" / "Use custom DNS suffix" + suffix textbox enable/disable interplay (mirrors DNS Lookup settings UI — should look/behave identically).
   - Actually resolve a bare hostname (e.g. via Ping or IP Scanner host range) with suffix enabled and confirm the suffix gets appended and resolution works; confirm FQDNs and PTR/reverse lookups are unaffected.
   - Test the settings-upgrade migration path: hand-edit/restore an old settings JSON with a populated `Network_CustomDNSServer` string and no `Network_CustomDNSServers`, bump the settings version below `2026.8.17.0`, launch, and confirm it migrates correctly into the new list at port 53.
3. **Confirm/adjust the migration version number** `2026.8.17.0` in `SettingsManager.cs` (`Upgrade()` dispatcher + `UpgradeTo_2026_8_17_0()`) to match the actual intended release version if it differs from today's date-based guess.
4. Consider whether `Website/docs/application/dns-lookup.md` or other doc pages should cross-reference the new global suffix setting (only `Website/docs/settings/network.md` was updated).
5. Nothing was committed — review the diff (`git diff`) and commit when satisfied.

## Explicit scope notes (things deliberately NOT done)

- DNS Lookup's own settings/behavior were **not changed** — only read as a reference pattern to copy.
- No new localization strings were added anywhere; every label reuses existing `Strings.*`/`StaticStrings.*` keys (chosen deliberately to avoid needing a Transifex sync for 17 languages). If that turns out to read awkwardly in the UI (e.g. the dialog's read-only "Name" field showing "DNS server(s)"), that's a place a dedicated new string could be added later.
- Global DNS servers remain **IP-only by design** (not a limitation to relax casually) — `DNSClient.Configure()` does a hard `IPAddress.Parse()`, so allowing hostnames there would require adding a resolution step first.
