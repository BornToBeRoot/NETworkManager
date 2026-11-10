# NETworkManager - Repository Guide for AI Agents

## Repository Overview

**NETworkManager** is a comprehensive, enterprise-ready network management and troubleshooting application built with C# and WPF. It provides a unified interface for network administrators and IT professionals to manage, monitor, and troubleshoot network infrastructure using tools like Remote Desktop, PuTTY (SSH, Telnet, Serial, etc.), PowerShell (k9s, WSL, etc.), TigerVNC, network scanners (IP scanner, Port scanner, WiFi Analyzer), and diagnostic utilities (Ping monitor, traceroute, etc.).

- **Repository**: https://github.com/BornToBeRoot/NETworkManager
- **License**: GNU General Public License v3
- **Primary Language**: C# (.NET 10)
- **UI Framework**: WPF (Windows Presentation Foundation) with `MahApps.Metro`
- **Documentation**: Docusaurus-based website and documentation at https://borntoberoot.net/NETworkManager
- **Platform**: Windows (x64 and ARM64)

## Project Structure

### Source Code Organization

The repository is organized into multiple C# projects following a modular architecture:

```
REPOSITORY_ROOT/
├── Source/                           # Main source code directory
│   ├── NETworkManager/               # Main WPF application (Views, ViewModels, Controls)
│   ├── NETworkManager.Models/        # Business logic and data models
│   ├── NETworkManager.Utilities/     # Helper classes and utilities
│   ├── NETworkManager.Utilities.WPF/ # WPF-specific utilities
│   ├── NETworkManager.Converters/    # WPF value converters
│   ├── NETworkManager.Validators/    # Input validation logic
│   ├── NETworkManager.Controls/      # Custom WPF controls
│   ├── NETworkManager.Localization/  # Internationalization resources
│   ├── NETworkManager.Settings/      # Application settings management
│   ├── NETworkManager.Profiles/      # Profile management
│   ├── NETworkManager.Update/        # Update functionality
│   ├── NETworkManager.Documentation/ # Documentation resources
│   ├── NETworkManager.Setup/         # WiX installer project
│   └── 3rdparty/                     # Third-party dependencies (e.g., Dragablz)
│       └── (managed-manually)        # NOTE: Maintained manually — AI agents MUST NOT modify this directory
├── Website/                          # Docusaurus documentation website
├── Scripts/                          # Build and automation scripts
│   ├── Create-FileHash.ps1           # Generate file hashes
│   ├── Create-FlagFromSVG.ps1        # Create flag images
│   ├── Create-LucideIcons.ps1        # Import the used Lucide icons (LucideIcons.cs) and their license
│   ├── Create-OUIListFromWeb.ps1     # Update OUI (MAC vendor) list
│   ├── Create-PortListFromWeb.ps1    # Update port list database
│   ├── Create-PublicSuffixListFromWeb.ps1  # Update public suffix list (RDAP: registered domain of a hostname)
│   ├── Create-ReleaseTable.ps1       # Create the SHA256 sums / file table for release notes
│   ├── Create-WhoisServerListFromWebAndWhois.ps1  # Update WHOIS servers
│   ├── Create-WorldMapFromWeb.ps1    # Create the world map (Traceroute map)
│   └── PreBuildEventCommandLine.ps1  # Pre-build automation
├── Tests/Environments/               # Test environments for manual testing (e.g. SNMP, Active Directory)
├── .claude/skills/                   # Skills for AI agents (e.g. feature-review with the app's UI/code conventions)
├── Chocolatey/                       # Chocolatey package definition
├── WinGet/                           # WinGet package manifest
└── Images/                           # Application icons and images
```

### Key Statistics

- **750+ C# source files** across multiple projects
- **160+ XAML files** for UI definitions
- **13 projects** in the solution (12 C# projects + the WiX installer)
- **70 documentation pages** in Docusaurus format
- **18 languages** (English + 17 translations)

## Technology Stack

### Application (.NET/WPF)

**Core Framework**:
- **.NET 10.0** (SDK version pinned in `Source/global.json`, updated by Dependabot)
- **WPF** (Windows Presentation Foundation) for UI
- **Windows Forms** integration for specific controls
- **Target framework**: `net10.0-windows` with `TargetPlatformVersion` `10.0.22621.0` (Windows SDK version, not in the TFM - otherwise Dependabot can't update the NuGet packages), runtime identifiers `win-x64` and `win-arm64`
- **Supported OS**: Windows 10 / Windows Server (22H2 or later) and Windows 11

**UI Framework & Design**:
- **MahApps.Metro** - Modern UI toolkit providing Metro-style controls and themes (restyled, see
  [UI Design System](#ui-design-system))
- **Lucide** - Icons (https://lucide.dev), imported from the SVG files by `Scripts/Create-LucideIcons.ps1` into
  `Source/NETworkManager.Controls/LucideIcons.cs` (only the used icons, see `$Icons` in the script) and drawn as lines by
  `LucideIcon` (stroke 1.75); license in `Source/NETworkManager.Documentation/Licenses/Lucide.txt`
- **MahApps.Metro.SimpleChildWindow** - Dialog windows
- **Dragablz** - Tearable/draggable tab control
- **GongSolutions.Wpf.DragDrop** (`gong-wpf-dragdrop`) - Drag and drop framework
- **Progress / loading** - MahApps `MetroProgressBar` (style `DefaultMetroProgressBar`) and `ProgressRing` in three sizes
  (`ProgressRingSmall` 16 inline, `ProgressRingMedium` 24 dialogs, `ProgressRingLarge` 48 centered), see
  `Resources/Styles/ProgressBarStyles.xaml`
- **ControlzEx** - Shared WPF controls (dependency of MahApps.Metro)

**Architecture Pattern**:
- **MVVM (Model-View-ViewModel)** pattern
  - Views: XAML files in `Source/NETworkManager/Views/`
  - ViewModels: C# classes in `Source/NETworkManager/ViewModels/`
  - Models: Business logic in `Source/NETworkManager.Models/`

**Key Libraries**:
- **Lextm.SharpSnmpLib** - SNMP protocol implementation
- **DnsClient** - DNS lookups
- **IPNetwork2** - Network and subnet calculations
- **PSDiscoveryProtocol** - LLDP/CDP network discovery (PowerShell module embedded)
- **AirspaceFixer** - WPF/WinForms interop fixes (code embedded as `AirspacePanel` in `NETworkManager.Controls`,
  based on AirspaceFixer and its fork AirspaceFixerDotNet)
- **log4net** - Logging framework
- **LiveChartsCore.SkiaSharpView.WPF** - Charts and graphs
- **Microsoft.Web.WebView2** - WebView2 control for web content
- **Microsoft.PowerShell.SDK** - PowerShell integration (e.g. Firewall, Neighbor Table)
- **Microsoft.Windows.CsWinRT** - Windows Runtime APIs (e.g. WiFi)
- **Microsoft.Xaml.Behaviors.Wpf** - Behaviors for XAML
- **Octokit** - GitHub API (update check)
- **System.Text.Json** - Settings and profile files
- **Newtonsoft.Json** - JSON export of results

**Build Tools**:
- **NetBeauty2** - Dependency organization
- **WiX Toolset** - MSI installer creation

### Website (Docusaurus)

**Documentation Stack**:
- **Docusaurus** (v3) - Static site generator
- **React** - UI framework
- **@mdx-js/react** - MDX support
- **react-image-gallery** - Image galleries
- **Prism** - Code syntax highlighting

**Website Structure**:
```
Website/
├── docs/                   # Documentation pages
│   ├── application/        # Feature documentation
│   ├── changelog/          # Version history
│   ├── faq/                # Frequently asked questions
│   ├── img/                # Screenshots used in the documentation
│   └── settings/           # Settings documentation
├── blog/                   # Blog posts
├── src/                    # React components and pages
├── static/                 # Static assets of the website (logo, favicon, images for pages/blog)
├── docusaurus.config.js    # Site configuration
├── sidebars.js             # Documentation sidebar structure
├── package.json            # npm/yarn dependencies
└── yarn.lock               # Yarn lockfile
```

## Development Setup

### Prerequisites

1. **For Application Development**:
   - [.NET 10.x SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
   - [Visual Studio 2026](https://visualstudio.microsoft.com/) with:
     - `.NET desktop development` workload
     - `Universal Windows Platform development` workload
   - Alternative: [JetBrains Rider](https://www.jetbrains.com/rider/) (still requires UWP workload via VS Installer)

2. **For Website Development**:
   - [Node.js](https://nodejs.org/) (LTS version recommended)
   - npm or yarn package manager (yarn is used in CI/CD)

### Building the Application

**Clone with Submodules**:
```powershell
git clone https://github.com/BornToBeRoot/NETworkManager.git
cd NETworkManager
git submodule update --init --recursive
```

**Build via Visual Studio/Rider**:
- Open `Source/NETworkManager.sln`
- Build the solution (F6 or Build → Build Solution)

**Build via Command Line**:
```powershell
dotnet restore .\Source\NETworkManager.sln
dotnet build .\Source\NETworkManager.sln --configuration Release --no-restore
```

### Building the Website

```bash
cd Website
npm install         # Install dependencies (or use yarn install)
npm start           # Start development server (or use yarn start)
npm run build       # Build for production (or use yarn build)
```

The website is deployed to GitHub Pages at: https://borntoberoot.net/NETworkManager

## CI/CD Pipeline

**Primary Build System**: AppVeyor (configuration in `appveyor.yml`)
- Builds on the Visual Studio 2022 image
- Installs the .NET SDK version from `Source/global.json` (skipped if the image already has it)
- Restores NuGet packages
- Builds the Release configuration: x64 for branches and PRs, x64 and ARM64 for tags (setup, portable and archive per
  architecture, only for tags). PRs only build the application (`NETworkManager.csproj`) unless they change
  `Source/NETworkManager.Setup/`, branches and tags the setup (`NETworkManager.Setup.wixproj`)
- Runs the unit tests (`Source/NETworkManager.Tests`, `dotnet test --project`)
- Branch builds only for `main` and release tags (`branches: only`), other branches are only built as PR
- Ends successfully without building if only docs / website / repository files changed (`Website/`, `.github/`,
  `.claude/`, `Images/`, `Chocolatey/`, `WinGet/`, `Tests/Environments/`, `*.md`, `LICENSE`), so Mergify still gets a
  green status. Tags are always built. Branch builds (e.g. `main`) only see the last commit, so they are only skipped
  for squash merges of a PR (subject ends with `(#1234)`); direct pushes are always built
- Version format: `yyyy.M.d.0` (date-based)
- Artifacts are signed via [SignPath.io](https://signpath.io/)
- Released to GitHub Releases

**GitHub Actions Workflows**:
- **CodeQL Analysis** (`.github/workflows/codeql.yml`):
  - Automated security scanning
  - Analyzes C# (autobuild on Windows), JavaScript/TypeScript and GitHub Actions workflows (Linux)
  - Runs weekly and on-demand via workflow_dispatch
  
- **Website Deployment** (`.github/workflows/deploy_website.yml`):
  - Builds and deploys Docusaurus website to GitHub Pages
  - Triggers on changes to `Website/` directory (push to `main` and PRs) and on published releases
  - Uses Node.js 22 with yarn for build
  - Uses Docusaurus Faster (`@docusaurus/faster`, Rspack / SWC) for faster builds; all `@docusaurus/*` packages must have
    the same version (Dependabot updates them as a group)
  - Build and deploy are separate jobs, only the deploy job (`main` / release) has write access

- **Label / Milestone** (`.github/workflows/label.yml`, `.github/workflows/milestone.yml`):
  - Label PRs from Transifex and ImgBot as dependencies (Dependabot sets the label itself)
  - Assign the next-release milestone to PRs from bots and the maintainer

**Automation & Bots**:
- **Dependabot** (`.github/dependabot.yml`):
  - Weekly updates for NuGet packages, .NET SDK, npm packages and GitHub Actions
  - Automatic PR creation for dependency updates, labeled `:heavy_check_mark: dependencies` (instead of Dependabot's
    default labels)
  
- **Mergify** (`.github/mergify.yml`):
  - Auto-merges PRs with `LGTM` label after AppVeyor CI passes
  - Auto-merges Dependabot (except major updates, they are merged manually after testing), Transifex, and ImgBot PRs
    after CI success
  - PRs that change `Website/` (or the website workflow) also need a successful `Build Website` check
  
- **Transifex Integration** (`.github/transifex.yml`):
  - Automated translation synchronization
  - Syncs RESX files between GitHub and Transifex

## Contributing Areas

### Code Contributions

**Primary Areas**:
1. **Network Tools** (`Source/NETworkManager.Models/`):
   - IP Scanner, Port Scanner, Ping Monitor
   - DNS Lookup, Traceroute, WHOIS
   - WiFi Analyzer, LLDP/CDP Capture

2. **Remote Access** (`Source/NETworkManager/Controls/`):
   - Remote Desktop (RDP) integration
   - PuTTY (SSH, Telnet, Serial) integration
   - PowerShell and TigerVNC (VNC) controls

3. **UI/UX** (`Source/NETworkManager/Views/` and `.xaml` files):
   - WPF views following MVVM pattern
   - MahApps.Metro styling
   - Custom controls and converters

4. **Settings & Profiles** (`Source/NETworkManager.Settings/`, `Source/NETworkManager.Profiles/`):
   - Profile management and encryption
   - Application settings

### Documentation Contributions

**Documentation Types**:
1. **User Documentation** (`Website/docs/`):
   - Feature guides in `docs/application/`
   - FAQ entries in `docs/faq/`
   - Settings documentation in `docs/settings/`

2. **Changelog** (`Website/docs/changelog/`):
   - Version release notes

3. **Code Documentation**:
   - XML documentation comments in C# code
   - README files in project directories

### Translation

- **Platform**: [Transifex](https://app.transifex.com/BornToBeRoot/NETworkManager/dashboard/)
- **Resources**: `Source/NETworkManager.Localization/`
- **17 translations** (plus English): cs-CZ, de-DE, es-ES, fr-FR, hu-HU, it-IT, ja-JP, ko-KR, lb-LU, nl-NL, pl-PL, pt-BR, ru-RU, sl-SI, uk-UA, zh-CN, zh-TW (enabled languages are listed in `LocalizationManager.cs`; other `Strings.*.resx` files may exist but are not offered yet)
- **Format**: RESX files. Only `Strings.resx` (English) is edited in the repository; translations come from Transifex.
- **Adding a string**: add it to `Strings.resx` and, in alphabetical order, to `Strings.Designer.cs`. Reuse existing strings where possible.
- **Integration**: Automated sync via `.github/transifex.yml`

## Code Style and Conventions

### C# Code Style

- **EditorConfig**: `.editorconfig` in `Source/` directory defines coding standards
- **Naming Conventions**:
  - PascalCase for classes, methods, properties
  - camelCase for private fields
  - Prefix interfaces with `I`
  - Suffix ViewModels with `ViewModel`
  
- **MVVM Pattern**:
  - ViewModels inherit from `ViewModelBase` (`PropertyChangedBase` + `CopyDataToClipboardCommand`)
  - Commands use `RelayCommand` from utilities
  - Views bind to ViewModels via DataContext
- **UI and code conventions** (commands, refresh/modify logic, admin rights, context menus, dialogs, tooltips,
  layout, PowerShell): see `.claude/skills/feature-review/references/conventions.md`

### XAML Style

- **MahApps.Metro Controls**: Use Metro-styled controls
- **Icons**: Lucide only - `{icons:Lucide Kind=Router}` (`xmlns:icons="clr-namespace:NETworkManager.Controls;assembly=NETworkManager.Controls"`),
  usually as `Visual` of a `VisualBrush` (opacity mask of a `Rectangle`), in C# `new LucideIcon { Kind = LucideIconKind.Router }`.
  A new icon: add its name to `$Icons` in `Scripts/Create-LucideIcons.ps1` and run the script (don't edit `LucideIcons.cs`)
- **Resource Dictionaries**: Shared styles in `Resources/Styles/` (tokens in `Theme.xaml`), see [UI Design System](#ui-design-system)
- **Naming**: Use `x:Name` for elements that need code-behind access

### Documentation Style

- **Markdown**: All documentation in `.md` or `.mdx` format
- **Code Blocks**: Use syntax highlighting with language specifiers
- **Screenshots**: Place in `Website/docs/img/` (referenced as `../img/<name>.png` from `docs/application/`)
- **Changelog**: Every user-visible change gets an entry in `Website/docs/changelog/next-release.md` with a PR or commit link
- **Links**: Use relative links for internal documentation

## Testing Strategy

- **Manual Testing**: Primary testing approach (test environments in `Tests/Environments/`)
- **Unit Tests**: xUnit tests in `Source/NETworkManager.Tests`, run by AppVeyor CI
- **Build Verification**: AppVeyor CI ensures builds succeed
- **Windows only**: The application only builds and runs on Windows (WPF, Windows SDK, CsWinRT). Changes made
  on other platforms can't be compiled and must be verified on Windows
- **Real-World Testing**: Pre-release versions on GitHub Releases
- **Community Feedback**: GitHub Issues for bug reports

## Package Distribution

**Installation Methods**:
1. **Direct Download**: GitHub Releases (Setup, Portable, Archive)
2. **Chocolatey**: `choco install networkmanager`
3. **WinGet**: `winget install BornToBeRoot.NETworkManager`
4. **Evergreen**: PowerShell module for automation

**Package Formats**:
- **Setup (MSI)**: WiX installer in `Source/NETworkManager.Setup/`
- **Portable**: ZIP file
- **Archive**: ZIP file

## Security Considerations

1. **Code Signing**: 
   - Free signing via SignPath.io and SignPath Foundation
   - All official binaries and installers are signed

2. **Profile Encryption**: 
   - Supports encrypted profile files (`.encrypted`)
   - Protects sensitive credentials and connection data

3. **Privacy**:
   - No telemetry or data collection by default
   - Optional third-party services (GitHub API, ipify.org, ip-api.com)

4. **Security Reports**: 
   - See `SECURITY.md` for vulnerability reporting process

## Important Files

- **`Source/NETworkManager.sln`**: Main Visual Studio solution
- **`Source/GlobalAssemblyInfo.cs`**: Shared assembly version info
- **`Source/global.json`**: .NET SDK version specification (also used by AppVeyor)
- **`Source/.editorconfig`**: C# code style configuration
- **`appveyor.yml`**: AppVeyor CI/CD configuration
- **`.github/workflows/codeql.yml`**: CodeQL security analysis workflow
- **`.github/workflows/deploy_website.yml`**: Website deployment workflow
- **`.github/workflows/label.yml`**, **`.github/workflows/milestone.yml`**: PR label / milestone automation
- **`.github/ISSUE_TEMPLATE/`**, **`.github/pull_request_template.md`**: Issue and PR templates
- **`.github/dependabot.yml`**: Dependabot configuration
- **`.github/mergify.yml`**: Mergify auto-merge configuration
- **`.github/transifex.yml`**: Transifex translation sync configuration
- **`.gitmodules`**: Git submodule configuration (Dragablz)
- **`Website/docusaurus.config.js`**: Documentation site configuration
- **`Website/docs/changelog/next-release.md`**: Changelog of the upcoming release
- **`.claude/skills/feature-review/`**: Feature review skill incl. the app's conventions (`references/conventions.md`)
- **`CONTRIBUTING.md`**: Contribution guidelines
- **`LICENSE`**: GPL v3 license
- **`README.md`**: Repository overview and quick start

## Useful Commands

### Application Development

```powershell
# Restore dependencies
dotnet restore .\Source\NETworkManager.sln

# Build (Release)
dotnet build .\Source\NETworkManager.sln --configuration Release

# Clean solution
dotnet clean .\Source\NETworkManager.sln

# Run application
.\Source\NETworkManager\bin\Release\net10.0-windows\win-x64\NETworkManager.exe
```

### Website Development

```bash
# Install dependencies
cd Website && npm install  # or yarn install

# Start dev server (http://localhost:3000)
npm start  # or yarn start

# Build static site
npm run build  # or yarn build

# Serve production build
npm run serve  # or yarn serve

# Clear cache
npm run clear  # or yarn clear
```

### Git Operations

```powershell
# Update submodules
git submodule update --init --recursive

# Check status
git status

# View changes
git diff
```

## Common Development Scenarios

### Adding a New Network Tool

1. Create model in `Source/NETworkManager.Models/` (e.g. `Network/`)
2. Create ViewModel in `Source/NETworkManager/ViewModels/` and View (XAML) in `Source/NETworkManager/Views/`
3. Add the tool to `ApplicationName` and `ApplicationManager` (`Source/NETworkManager.Models/`), and show/hide it
   in `MainWindow.xaml.cs` (`OnApplicationViewVisible` / `OnApplicationViewHide`)
4. Add it to the application list for existing users in `SettingsManager` (upgrade step)
5. If the tool accepts a target or profile: implement `IApplicationRequestHandler` in its (host) view and add a run
   command in `RunCommandManager` (command, aliases, validator = the one of the input field; suggestions in
   `TargetApplications`), see [Navigation and Run Command](#navigation-and-run-command)
6. Add documentation in `Website/docs/application/` (and the command table in `Website/docs/run-command.md`) and map it
   in `DocumentationManager`
7. Add localization strings and a changelog entry

### Adding a New Setting

1. Add the property `<Tool>_<Name>` to `SettingsInfo` and its default to `GlobalStaticConfiguration`
   (`Source/NETworkManager.Settings/`)
2. Add the UI to the tool's settings page (`<Tool>SettingsView.xaml` + `<Tool>SettingsViewModel.cs`)
3. If the tool has no settings page yet: create both and register it in `SettingsViewManager`
   (`SettingsName.<Tool>`), `SettingsViewModel` (switch) and `DocumentationManager`
4. If the tool must react while it is open, subscribe to `SettingsManager.Current.PropertyChanged` in the tool's
   ViewModel (reference: `FirewallViewModel`)
5. Add a help icon/tooltip (`HelpImageRectangle` + `HelpMessage_*` string) if the option needs explanation
6. Document it in the `## Settings` section of `Website/docs/application/<tool>.md` and add a changelog entry

### Updating Documentation

1. Edit markdown files in `Website/docs/`
2. Add images to `Website/docs/img/`
3. Update `Website/sidebars.js` if adding new sections
4. Test locally with `npm start`
5. Submit pull request

## Resources

- **Documentation**: https://borntoberoot.net/NETworkManager
- **GitHub Issues**: https://github.com/BornToBeRoot/NETworkManager/issues
- **GitHub Discussions**: https://github.com/BornToBeRoot/NETworkManager/discussions
- **Transifex**: https://app.transifex.com/BornToBeRoot/NETworkManager/dashboard/
- **AppVeyor CI**: https://ci.appveyor.com/project/BornToBeRoot/NETworkManager

## Key Contacts & Community

- **Maintainer**: BornToBeRoot
- **Contributors**: See `CONTRIBUTORS.md`
- **Code of Conduct**: `CODE_OF_CONDUCT.md` (Contributor Covenant)
- **Support**: GitHub Issues and Discussions

## Architecture Highlights

### MVVM Implementation

- **Data Binding**: Extensive use of WPF data binding
- **Commands**: `RelayCommand` pattern for user actions
- **Property Change Notification**: `INotifyPropertyChanged` via `PropertyChangedBase`
- **Dependency Injection**: Manual dependency management (no DI container)

### UI Design System

Calm, productive look on top of MahApps.Metro (no WPF Fluent / WPF-UI). Use the existing tokens and styles below instead
of local colors, sizes or templates; theme-dependent brushes always with `DynamicResource`.

**Themes and accents** (`NETworkManager.Settings`):
- MahApps themes `Light` / `Dark` plus own themes from `ThemeVariants`: dark Black, Midnight, Steel, Indigo (copies of
  `Dark.<accent>`, neutral grays tinted toward the theme background) and light Mist, Soft, Paper (copies of
  `Light.<accent>` with their own frame color). Own accents Sky, Ocean, Mint, Rose, Coral, Gold, Lavender, Slate,
  Graphite (copies of `<theme>.Blue` with the accent replaced). They are created on demand in
  `AppearanceManager.ChangeTheme` (`LibraryTheme` + `ThemeManager.AddLibraryTheme`); on failure the default theme is used.
  A theme counts as dark by the brightness of its background (`ThemeVariants.IsDark`), not by its name.
- Default accent for new installations: Teal. Custom themes (`Appearance_UseCustomTheme`) keep working.
- Settings: `Appearance_Theme`, `Appearance_Accent`, `Appearance_AccentGlowIntensity` (Off / Subtle 6 % / Medium 10 %
  (default) / Strong 15 %), `Appearance_Animations` (System / On / Off; policy `Appearance_Animations` in `config.json`
  is a bool).
- Console colors: `AppearanceManager.ConsoleBackground` / `ConsoleForeground` / `ConsoleBoldForeground` (event
  `ConsoleColorsChanged`) are written to the PuTTY / PowerShell profiles, so the embedded consoles match the theme.

**Theme-dependent brushes** - computed by `AppearanceManager` on every theme change (from `MahApps.Colors.ThemeForeground`
/ `ThemeBackground` / `Accent`) and written into `Application.Current.Resources`:

| Key | Value | Use |
|---|---|---|
| `Brushes.Surface.Frame` | dark: background 25 % darker, light: 4 % foreground (or the theme's own frame) | window frame, title bar, navigation |
| `Brushes.Surface.Window` / `.Inactive` | frame + accent glow (radial, top left), half glow when the window is inactive | `FrameWindow` background |
| `Brushes.Surface.Border` | foreground 10 % | all lines and surface borders: boxes, cards, separators, tabs, grid headers, splitters, popups, tooltips, dialogs, group boxes (input / check box / radio borders keep `MahApps.Brushes.Gray8` for contrast) |
| `Brushes.Navigation.Selected` | accent 15 % | selected tool in the navigation |
| `Brushes.Control.MouseOver` / `.Pressed` | foreground 8 % / 12 % | hover / pressed of icon buttons, lists, tabs |
| `Brushes.Control.Border.MouseOver` | foreground 35 % | input border on hover |
| `Brushes.Button.Background` / `.MouseOver` / `.Pressed` | foreground 6 / 10 / 14 % | secondary buttons, title bar fields |
| `Brushes.Status.Success` / `.Warning` / `.Error` | dark: `#badc58` / `#f1b856` / `#e5605a`, light: `#558b16` / `#b06d00` / `#b95353` | status icons and texts, validation - never hard-code `#badc58` etc. |
| `MahApps.Brushes.DataGrid.Selection.*` | accent 30 % (selected), 38 % (selected + hover), hover foreground 8 % | data grids |

Charts use `ChartColors` (theme-aware primary / secondary / WiFi palette, gradient `AreaFill`).

**Theme-independent resources** - `Resources/Styles/Theme.xaml` (merged first):
- Fonts: `Segoe UI Variable Text, Segoe UI` for the MahApps control / button / toggle switch fonts and `DefaultWindow`;
  `Fonts.Family.Mono` (`Cascadia Mono, Consolas`) for technical values: IP / MAC addresses, hostnames, ports, subnets,
  gateways, DNS / DHCP / SNTP servers, BSSID, OIDs, URLs, DNS records, raw output (inputs with such a watermark too).
  Names, descriptions, vendors, states, times, user names and paths use the UI font.
- Corner radii: `CornerRadius.Control` 4 (buttons, inputs, menus, tooltips), `.Control.Inner` 3, `.Overlay` 8
  (dialogs, popups), `.Card` 8 (boxes, cards). `Brushes.Overlay` (black 40 %, dialog overlay). `FocusVisual.Ring`
  (gray 2 px ring, also the default focus visual - no accent).

**Type scale** (`TextBlockStyles.xaml`): `DefaultTextBlock` 14 (also navigation, tab titles), `HeaderTextBlock` 16
SemiBold (section headers), `MessageTextBlock` 16 gray (messages in empty areas), mono via `Fonts.Family.Mono`.

**Layout and surfaces**:
- Box layout: the window frame (`FrameWindow` style for main, tab host and status window) holds the navigation (frame
  color, no border) and rounded boxes for the main view and the profiles (`ViewBox`: theme background,
  `Brushes.Surface.Border`, radius 8; gap 6). Host views set `ViewLayoutHelper.HasOwnBoxes` (main view box transparent).
- Cards (Dashboard widgets, Ping Monitor hosts): `CardBorder` (border only, no fill). Groups: `SectionExpander` (section
  header, no box in box).
- Headers 36 high (navigation header, Dragablz tabs, `DefaultTabItem`). Tabs: VS Code-like (shared styles
  `DragablzTabHeaderBorder` / `Title` / `CloseIcon` / `AccentLine` in `DragablzStyles.xaml` - change the tab look only
  there); a single tab is shown as the view title.
- Data grids: row height 30, semibold headers, accent-tinted selection.
- Windows have rounded corners on Windows 11 (`WindowCornerHelper.RoundCorners`, DWM).

**Controls**:
- Buttons: `HighlightedButton` (accent, the main action, e.g. **Scan**), `DefaultButton` / `ImageWithTextButton`
  (subtle fill, min. width 100), `ImageButton` / `ImageDropDrownButton` (transparent icon buttons). Template
  `DefaultButtonTemplate` with fading hover / pressed layers (`ButtonHelper.MouseOverOverlay` / `PressedOverlay`).
- Disabled = `Opacity 0.55` (inputs, buttons, CheckBox / RadioButton / ToggleSwitch keep their colors), no MahApps veil.
  Read-only inputs while a task runs: `IsReadOnly` + `ComboBoxReadOnlyBehavior` instead of disabled.
- Inputs: radius 4, focus border gray (`Gray5`), hover border `Brushes.Control.Border.MouseOver`.
- Drop-downs: `ComboBoxPopupBehavior` (rounded popup with shadow), implicit `ComboBoxItem` style in `DefaultComboBox`.
- Progress: `DefaultMetroProgressBar` (accent on a subtle track) and `ProgressRingSmall` / `Medium` / `Large`
  (16 inline / 24 dialogs / 48 centered, `ProgressBarStyles.xaml`); scanner progress via `SmoothProgress.Value`.
- Dialogs: `DefaultChildWindow` (own template, radius 8, shadow, overlay). Welcome / Upgrade keep their own overlay.
- Status values in lists (e.g. firewall action, TCP / neighbor state): `StatusDot` (8 px `Ellipse` in front of the text,
  `Tag` = `StatusLevel` None / Success / Warning / Error via a `*ToStatusLevelConverter`), text in the normal color.
  Template columns set `ClipboardContentBinding` to keep the text for copying.
- Empty areas (no results, missing prerequisite, restart as administrator): `EmptyStateMessage` (`Icon`, `Text`,
  `IconSize` 40, 24 for notices below a list) instead of a bare `MessageTextBlock`; status lines and loading texts stay
  `MessageTextBlock`.
- Template copies of MahApps / SimpleChildWindow (keep in sync when updating these packages): ComboBox toggle,
  ToolTip, ContextMenu / submenu, scrollbar thumb, `DefaultChildWindowTemplate`, `MahApps.Styles.DataGridRow`
  (redefined with the same key).

**Icons**: Lucide only (see [XAML Style](#xaml-style)); 16 × 16 `Rectangle` with `MahApps.Brushes.Gray3` fill and the icon
as opacity mask (`VisualBrush`), stroke 1.75 (`LucideIcon.DefaultStrokeThickness`). Icons that cover only a
small part of the 24 box (plus, minus, X, chevrons, arrows) are drawn 1.25 times larger (`LucideIcon.SmallIcons`). Icons of the context menu actions:
see `.claude/skills/feature-review/references/conventions.md`.

**Motion** (only `Opacity` / `RenderTransform`, 150–250 ms, quadratic ease-out, no bounce, no staggered lists, **no view /
content transitions** when switching tools; check `AppearanceManager.AnimationsEnabled` when an animation starts):
`ChildWindowAnimation` (dialogs zoom 92 → 100 %, run command `Kind="SlideDown"`), `SelectionIndicatorAnimation`
(navigation indicator), `ExpanderAnimation` (chevron rotates, content fades in; `SwapChevron` for the profile panel),
`SmoothProgress`, tooltip fade, notification slide-in. Native windows (Remote Desktop, PuTTY, PowerShell, TigerVNC,
WebView2) can't be animated, clipped or put in transparent windows.

**Accent glow**: only in the window frame (`Brushes.Surface.Window`), not on dialogs, overlays or controls.

### Profile System

- **Profile Files**: JSON-based profile storage (legacy XML files are migrated automatically); settings are stored as JSON too
- **Encryption**: AES encryption for sensitive data (encrypted profile files use the `.encrypted` extension)
- **Groups**: Hierarchical organization
- **Import/Export**: Profile sharing capabilities

### Navigation and Run Command

- **Open a tool from code**: `NavigationManager.Open(new ApplicationRequest(ApplicationName.X) { Target = ... })` or a
  derived request with extra options (`ApplicationRequest.cs`: `PortScannerRequest`, `DNSLookupRequest`, `RDAPRequest`,
  `WakeOnLANRequest`, `SubnetCalculatorRequest`, `LookupRequest`). Used for "open in other tool" context menus
  (e.g. IP Scanner, Traceroute, Network Interface) and the run command. `Profile` runs a profile, `Start = false` only fills in the input.
- **MainWindow** switches to the tool and calls `IApplicationRequestHandler.Open(request)` on its view (host views call
  `AddTab(...)`). Don't reference other tools' views or view models directly.
- **Run command** (`Ctrl+Shift+P`, title bar box, optional global hotkey): parser in
  `Source/NETworkManager.Models/RunCommand/`, commands, validation and requests in `Source/NETworkManager/RunCommandManager.cs`,
  dialog in `RunCommandViewModel` / `RunCommandChildWindow`, docs in `Website/docs/run-command.md`. The run command must
  validate a target with the same validator as the tool's input field and profile dialog.
- **Large lists**: `ObservableRangeCollection<T>.ReplaceAll(items)` (`Source/NETworkManager.Controls/`) replaces all items
  with a single `Reset` notification. Use it for lists that are reloaded completely (e.g. Firewall rules, run command
  suggestions); not for results that are appended while a scan runs (those are buffered and flushed by a timer, see
  `IPScannerViewModel`).

### Embedded Tools Integration

- **Remote Desktop**: Native RDP using `AxMSTSCLib`, `AxMsRdpClient10NotSafeForScripting` and `WindowsFormsHost`
- **PuTTY**: PuTTY window embedding / process hosting with `WindowsFormsHost`
- **PowerShell**: PowerShell window embedding / process hosting with `WindowsFormsHost`
- **VNC**: TigerVNC window embedding / process hosting with `WindowsFormsHost`

## Tips for AI Agents

1. **Respect MVVM**: Keep business logic in ViewModels and Models, not in code-behind
2. **Use MahApps Controls**: Prefer Metro controls over standard WPF controls
3. **Localization**: Always use resource strings, never hardcode text (also for messages created in Models, e.g. map exceptions to localized strings in the ViewModel)
4. **Follow Existing Patterns**: Study similar features before implementing new ones
5. **Update Documentation**: Changes to features should include doc updates and a changelog entry in `Website/docs/changelog/next-release.md`
6. **Test on Real Networks**: Network tools require real network testing
7. **Version Management**: Assembly version is auto-generated in CI/CD
8. **Submodules**: Remember to init/update submodules (Dragablz in `Source/3rdparty/`)
9. **DO NOT Modify 3rdparty**: The `Source/3rdparty/` directory contains the Dragablz submodule and is maintained manually — AI agents MUST NOT modify this directory
10. **Automation Scripts**: Use scripts in `Scripts/` directory for updating lists (OUI, ports, WHOIS servers)
11. **Check CI Status**: Both AppVeyor (main build) and GitHub Actions (CodeQL, website) must pass
12. **Dependency Updates**: Dependabot automatically creates PRs for dependency updates
13. **Conventions and reviews**: Read `.claude/skills/feature-review/references/conventions.md` before changing UI or ViewModel code; use the `feature-review` skill to review a whole tool
14. **Plans and reviews**: Local (not committed) plans and review results are in `.claude/plans/` and `.claude/reviews/`
    (e.g. `.claude/reviews/ip-scanner.md`). Read the file for the area you work on first — it lists verified findings,
    decisions and open items
15. **Windows only**: The solution can't be built on Linux/macOS (WPF, CsWinRT) — say so when changes couldn't be compiled, and list what needs to be tested on Windows

## Glossary

- **MVVM**: Model-View-ViewModel architectural pattern
- **WPF**: Windows Presentation Foundation (Microsoft's UI framework)
- **MahApps.Metro**: Modern UI toolkit for WPF applications
- **Dragablz**: Third-party tab control with drag/drop support
- **LLDP/CDP**: Link Layer Discovery Protocol / Cisco Discovery Protocol
- **SNMP**: Simple Network Management Protocol
- **RDP**: Remote Desktop Protocol
- **Docusaurus**: React-based static site generator
- **SignPath**: Code signing service for open source projects
