---
sidebar_position: 0
---

# Next Release

Version: **Next release** <br />
Release date: **xx.xx.2025**

| File | `SHA256` |
| ---- | -------- |

**System requirements**

- Windows 10 / Server x64 (22H2 or later)
- [.NET Desktop Runtime 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime)

## Breaking Changes

- Migrated from .NET 8.0 (LTS) to .NET 10.0 (LTS).
  Upgrade your [.NET Desktop Runtime to version 10.0 (LTS) - x64](https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime) before you install this version. [#3229](https://github.com/BornToBeRoot/NETworkManager/pull/3229)

- `AWS Session Manager` feature has been removed. The [AWS Session Manager Plugin](https://github.com/aws/session-manager-plugin) is not actively maintained and contains several bugs (e.g. German / Spain keyboard layout issues). The current code base was also difficult to maintain and extend, and I currently have no test environment.
  The Sync feature (EC2 instances -> Profiles) has been removed as well, because it was limited to AWS Session Manager only. This will be re-introduced in a future release to support multiple providers (`AWS`, `Azure`, etc.) and more features like `Ping Monitor`, `PuTTY` or `Remote Desktop`.

  :::note

  You can still use AWS Session Manager within NETworkManager with the PowerShell feature by using the `aws ssm start-session --target <instance-id>` command.

  :::

## What's new?

- New language Ukrainian (`uk-UA`) has been added. Thanks to [@vadickkt](https://github.com/vadickkt) [#3240](https://github.com/BornToBeRoot/NETworkManager/pull/3240)
- Migrated all dialogs to child windows for improved usability and accessibility. [#3271](https://github.com/BornToBeRoot/NETworkManager/pull/3271)

**DNS Lookup**

- Allow direct dns server input (`<hostname>|<ipadress>:<port>` - `<port>` is optional) in the lookup view in addition to select from a list of configured servers. [#3261](https://github.com/BornToBeRoot/NETworkManager/pull/3261)
  See the [DNS Lookup](../application/dns-lookup.md) documentation for more information.

**Remote Desktop**

- Flag to enable `Admin (console) session` added to the RDP connect, profile and group dialogs. This flag allows connecting to the console session of the remote computer. [#3216](https://github.com/BornToBeRoot/NETworkManager/pull/3216)

## Improvements

**Profiles**

- Profile file creation flow improved — when adding a new profile you are now prompted to enable profile-file encryption to protect stored credentials and settings. [#3227](https://github.com/BornToBeRoot/NETworkManager/pull/3227)
- Profile file dialog migrated to a child window to improve usability. [#3227](https://github.com/BornToBeRoot/NETworkManager/pull/3227)
- Credential dialogs migrated to child windows to improve usability. [#3231](https://github.com/BornToBeRoot/NETworkManager/pull/3231)

**Settings**

- Settings format migrated from `XML` to `JSON`. The settings file will be automatically converted on first start after the update. [#3282](https://github.com/BornToBeRoot/NETworkManager/pull/3282)
- Create a daily backup of the settings file before saving changes. Up to `10` backup files are kept in the `Backups` subfolder of the settings directory. [#3283](https://github.com/BornToBeRoot/NETworkManager/pull/3283)

**IP Scanner**

- Improved local subnet detection: If local IP cannot be determined via routing, now iterates over active network adapters and selects the first valid IPv4 address (with link-local addresses (`169.254.x.x`) given lower priority). [#3288](https://github.com/BornToBeRoot/NETworkManager/pull/3288)

**DNS Lookup**

- Allow hostname as server address in addition to IP address in the add/edit server dialog. [#3261](https://github.com/BornToBeRoot/NETworkManager/pull/3261)
- Add `quad9` to the predefined DNS server list. [#3261](https://github.com/BornToBeRoot/NETworkManager/pull/3261)
- Redesign add/edit server dialog (migrated from dialog to child window). [#3261](https://github.com/BornToBeRoot/NETworkManager/pull/3261)

**Remote Desktop**

- Redesign RDP connect dialog (migrated from dialog to child window). [#3216](https://github.com/BornToBeRoot/NETworkManager/pull/3216)

**PowerShell**

- Redesign PowerShell connect dialog (migrated from dialog to child window). [#3234](https://github.com/BornToBeRoot/NETworkManager/pull/3234)

**PuTTY**

- Redesign PuTTY connect dialog (migrated from dialog to child window). [#3234](https://github.com/BornToBeRoot/NETworkManager/pull/3234)

**Web Console**

- Redesign Web Console connect dialog (migrated from dialog to child window). [#3234](https://github.com/BornToBeRoot/NETworkManager/pull/3234)

**SNTP Lookup**

- Redesign add/edit server dialog (migrated from dialog to child window). [#3261](https://github.com/BornToBeRoot/NETworkManager/pull/3261)

## Bug Fixes

**IP Scanner**

- Fix race condition when scan is complete but not all results have been processed yet, causing a wrong error message to be displayed. [#3287](https://github.com/BornToBeRoot/NETworkManager/pull/3287)
- Fix potential error in export logic if some data is null. [#3290](https://github.com/BornToBeRoot/NETworkManager/pull/3290)
- Fix missing simicolon separators in CSV output. [#3290](https://github.com/BornToBeRoot/NETworkManager/pull/3290)

**Port Scanner**

- Fix race condition when scan is complete but not all results have been processed yet, causing a wrong error message to be displayed. [#3287](https://github.com/BornToBeRoot/NETworkManager/pull/3287)

**PowerShell**

- Resolve the actual path to `pwsh.exe` under `C:\Program Files\WindowsApps\` instead of relying on the stub located at `%LocalAppData%\Microsoft\WindowsApps\`. The stub simply redirects to the real executable, and settings such as themes are applied only to the real binary via the registry. [#3246](https://github.com/BornToBeRoot/NETworkManager/pull/3246)
- The new profile filter popup introduced in version `2025.10.18.0` was instantly closed when a `PowerShell` session was opened and the respective application / view was selected. [#3219](https://github.com/BornToBeRoot/NETworkManager/pull/3219)

**PuTTY**

- The new profile filter popup introduced in version `2025.10.18.0` was instantly closed when a `PuTTY` session was opened and the respective application / view was selected. [#3219](https://github.com/BornToBeRoot/NETworkManager/pull/3219)

**SNMP**

- Fix `NullReferenceException` when no SNMP profile is selected in the DataGrid and `Return` key is pressed. [#3271](https://github.com/BornToBeRoot/NETworkManager/pull/3271)

## Dependencies, Refactoring & Documentation

- Documentation updated
- Code cleanup & refactoring
  - Introduced a new DialogHelper utility to centralize creation of `OK` and `OK/Cancel` dialogs. This reduces duplication and enforces a consistent layout. [#3231](https://github.com/BornToBeRoot/NETworkManager/pull/3231)
  - Migrate RegexHelper to precompiled Regex patterns for improved performance. [#3261](https://github.com/BornToBeRoot/NETworkManager/pull/3261)
- Update Ports, OUIs, Whois servers [#3274](https://github.com/BornToBeRoot/NETworkManager/pull/3274)
- Migrated existing credential and profile dialogs to use DialogHelper, improving usability, accessibility and maintainability across the app. [#3231](https://github.com/BornToBeRoot/NETworkManager/pull/3231)
- Language files updated via [#transifex](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Ftransifex-integration)
- Dependencies updated via [#dependabot](https://github.com/BornToBeRoot/NETworkManager/pulls?q=author%3Aapp%2Fdependabot)
- Add Junie/Gemini 3 generated XML documentation headers for roughly half of the ViewModels. This might help with readability. [#3251](https://github.com/BornToBeRoot/NETworkManager/pull/3251)
- Generate project guidelines for Rider's coding agent Junie using itself as generator. [#3250](https://github.com/BornToBeRoot/NETworkManager/pull/3250)
