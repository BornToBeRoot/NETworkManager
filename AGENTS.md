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
- **MahApps.Metro** - Modern UI toolkit providing Metro-style controls and themes
- **MahApps.Metro.IconPacks** - Icon libraries (FontAwesome, Material, Modern, Octicons)
- **MahApps.Metro.SimpleChildWindow** - Dialog windows
- **Dragablz** - Tearable/draggable tab control
- **GongSolutions.Wpf.DragDrop** (`gong-wpf-dragdrop`) - Drag and drop framework
- **Loading indicators** - Own implementation in `Source/NETworkManager/LoadingIndicators.cs` (styles e.g. `LoadingIndicatorArcsStyle`, `LoadingIndicatorPulseStyle`)
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
- **AirspaceFixer** - WPF/WinForms interop fixes
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
  architecture, only for tags). PRs only build the application (`NETworkManager.csproj`), branches and tags the setup
  (`NETworkManager.Setup.wixproj`)
- Runs the unit tests (`Source/NETworkManager.Tests`, `dotnet test --project`)
- Branches with an open PR are only built as PR (`skip_branch_with_pr`)
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
  - Label PRs from Dependabot, Transifex and ImgBot as dependencies
  - Assign the next-release milestone to PRs from bots and the maintainer

**Automation & Bots**:
- **Dependabot** (`.github/dependabot.yml`):
  - Weekly updates for NuGet packages, .NET SDK, npm packages and GitHub Actions
  - Automatic PR creation for dependency updates
  
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
- **Icon Packs**: Use `IconPacks` namespace for icons
- **Resource Dictionaries**: Shared styles in `Resources/` directory
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

### UI Theming

- **MahApps.Metro Themes**: Light/Dark theme support
- **Accent Colors**: Customizable accent colors
- **Resource Dictionaries**: Shared styles and templates
- **Live Theme Switching**: Runtime theme changes

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
