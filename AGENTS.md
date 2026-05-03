# NETworkManager - Repository Guide for AI Agents

## Repository Overview

**NETworkManager** is a comprehensive, enterprise-ready network management and troubleshooting application built with C# and WPF. It provides a unified interface for network administrators and IT professionals to manage, monitor, and troubleshoot network infrastructure using tools like Remote Desktop, PuTTY (SSH, Telnet, Serial, etc.), PowerShell (k9s, WSL, etc.), TigerVNC, network scanners (IP scanner, Port scanner, WiFi Analyzer), and diagnostic utilities (Ping monitor, traceroute, etc.).

- **Repository**: https://github.com/BornToBeRoot/NETworkManager
- **License**: GNU General Public License v3
- **Primary Language**: C# (.NET 10)
- **UI Framework**: WPF (Windows Presentation Foundation) with `MahApps.Metro`
- **Documentation**: Docusaurus-based website and documentation at https://borntoberoot.net/NETworkManager
- **Platform**: Windows (x64)

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
│   ├── Create-WhoisServerListFromWebAndWhois.ps1  # Update WHOIS servers
│   └── PreBuildEventCommandLine.ps1  # Pre-build automation
├── Chocolatey/                       # Chocolatey package definition
├── WinGet/                           # WinGet package manifest
└── Images/                           # Application icons and images
```

### Key Statistics

- **650+ C# source files** across multiple projects
- **150+ XAML files** for UI definitions
- **12 distinct project modules** in the solution
- **64+ documentation pages** in Docusaurus format
- **17 supported languages** for localization

## Technology Stack

### Application (.NET/WPF)

**Core Framework**:
- **.NET 10.0** (SDK version: 10.0.100)
- **WPF** (Windows Presentation Foundation) for UI
- **Windows Forms** integration for specific controls
- **Target**: Windows 10.0.22621.0 (Windows 11)

**UI Framework & Design**:
- **MahApps.Metro** - Modern UI toolkit providing Metro-style controls and themes
- **MahApps.Metro.IconPacks** - Icon libraries (FontAwesome, Material, Modern, Octicons)
- **MahApps.Metro.SimpleChildWindow** - Dialog windows
- **Dragablz** - Tearable/draggable tab control
- **GongSolutions.Wpf.DragDrop** - Drag and drop framework
- **LoadingIndicators.WPF** - Loading animations
- **ControlzEx** - Shared WPF controls

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
- **LiveCharts.Wpf** - Charts and graphs
- **Microsoft.Web.WebView2** - WebView2 control for web content
- **Microsoft.PowerShell.SDK** - PowerShell integration
- **Microsoft.Xaml.Behaviors.Wpf** - Behaviors for XAML
- **Newtonsoft.Json** - JSON serialization and deserialization

**Build Tools**:
- **NetBeauty2** - Dependency organization
- **WiX Toolset** - MSI installer creation

### Website (Docusaurus)

**Documentation Stack**:
- **Docusaurus** - Static site generator
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
│   └── settings/           # Settings documentation
├── blog/                   # Blog posts
├── src/                    # React components and pages
├── static/                 # Static assets (images, files)
├── docusaurus.config.js    # Site configuration
├── sidebars.js             # Documentation sidebar structure
├── package.json            # npm/yarn dependencies
└── yarn.lock               # Yarn lockfile
```

## Development Setup

### Prerequisites

1. **For Application Development**:
   - [.NET 10.x SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) or later with:
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
- Builds on Visual Studio 2022 image
- Installs .NET 10.0.100 SDK
- Restores NuGet packages
- Builds Release configuration for x64 platform
- Version format: `yyyy.M.d.0` (date-based)
- Artifacts are signed via [SignPath.io](https://signpath.io/)
- Released to GitHub Releases

**GitHub Actions Workflows**:
- **CodeQL Analysis** (`.github/workflows/codeql.yml`):
  - Automated security scanning
  - Analyzes C# and JavaScript/TypeScript code
  - Runs weekly and on-demand via workflow_dispatch
  
- **Website Deployment** (`.github/workflows/deploy_website.yml`):
  - Builds and deploys Docusaurus website to GitHub Pages
  - Triggers on changes to `Website/` directory
  - Uses Node.js 20 with yarn for build

**Automation & Bots**:
- **Dependabot** (`.github/dependabot.yml`):
  - Weekly updates for NuGet packages, .NET SDK, and npm packages
  - Automatic PR creation for dependency updates
  
- **Mergify** (`.github/mergify.yml`):
  - Auto-merges PRs with `LGTM` label after AppVeyor CI passes
  - Auto-merges Dependabot, Transifex, and ImgBot PRs after CI success
  
- **Transifex Integration** (`.github/transifex.yml`):
  - Automated translation synchronization
  - Syncs RESX files between GitHub and Transifex
  
- **Stale Bot** (`.github/stale.yml`):
  - Marks issues inactive for 30 days as stale
  - Closes stale issues after 7 days
  - Excludes bugs, feature requests, and documentation issues

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
- **17 languages supported**: cs-CZ, de-DE, es-ES, fr-FR, hu-HU, it-IT, ja-JP, ko-KR, nl-NL, pl-PL, pt-BR, ru-RU, sl-SI, sv-SE, uk-UA, zh-CN, zh-TW
- **Format**: RESX files
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
  - ViewModels inherit from `PropertyChangedBase` or implement `INotifyPropertyChanged`
  - Commands use `RelayCommand` from utilities
  - Views bind to ViewModels via DataContext

### XAML Style

- **MahApps.Metro Controls**: Use Metro-styled controls
- **Icon Packs**: Use `IconPacks` namespace for icons
- **Resource Dictionaries**: Shared styles in `Resources/` directory
- **Naming**: Use `x:Name` for elements that need code-behind access

### Documentation Style

- **Markdown**: All documentation in `.md` or `.mdx` format
- **Code Blocks**: Use syntax highlighting with language specifiers
- **Screenshots**: Place in `Website/static/img/`
- **Links**: Use relative links for internal documentation

## Testing Strategy

- **Manual Testing**: Primary testing approach
- **Build Verification**: AppVeyor CI ensures builds succeed
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
   - Supports encrypted profile files
   - Protects sensitive credentials and connection data

3. **Privacy**:
   - No telemetry or data collection by default
   - Optional third-party services (GitHub API, ipify.org, ip-api.com)

4. **Security Reports**: 
   - See `SECURITY.md` for vulnerability reporting process

## Important Files

- **`Source/NETworkManager.sln`**: Main Visual Studio solution
- **`Source/GlobalAssemblyInfo.cs`**: Shared assembly version info
- **`Source/global.json`**: .NET SDK version specification (10.0.100)
- **`Source/.editorconfig`**: C# code style configuration
- **`appveyor.yml`**: AppVeyor CI/CD configuration
- **`.github/workflows/codeql.yml`**: CodeQL security analysis workflow
- **`.github/workflows/deploy_website.yml`**: Website deployment workflow
- **`.github/dependabot.yml`**: Dependabot configuration
- **`.github/mergify.yml`**: Mergify auto-merge configuration
- **`.github/transifex.yml`**: Transifex translation sync configuration
- **`.github/stale.yml`**: Stale issue bot configuration
- **`.gitmodules`**: Git submodule configuration (Dragablz)
- **`Website/docusaurus.config.js`**: Documentation site configuration
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
.\Source\NETworkManager\bin\Release\net10.0-windows10.0.22621.0\win-x64\NETworkManager.exe
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

1. Create model in `Source/NETworkManager.Models/Network/`
2. Create ViewModel in `Source/NETworkManager/ViewModels/`
3. Create View (XAML) in `Source/NETworkManager/Views/`
4. Register in `MainWindow.xaml.cs` or appropriate host
5. Add localization strings
6. Update documentation in `Website/docs/application/`

### Adding a New Setting

1. Add property to settings model in `Source/NETworkManager.Settings/`
2. Update settings ViewModel
3. Add UI control in settings View
4. Add validation if needed
5. Update settings documentation

### Updating Documentation

1. Edit markdown files in `Website/docs/`
2. Add images to `Website/static/img/`
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

- **Profile Files**: XML-based profile storage
- **Encryption**: AES encryption for sensitive data
- **Groups**: Hierarchical organization
- **Import/Export**: Profile sharing capabilities

### Embedded Tools Integration

- **Remote Desktop**: Native RDP using `AxMSTSCLib`, `AxMsRdpClient10NotSafeForScripting` and `WindowsFormsHost`
- **PuTTY**: PuTTY window embedding / process hosting with `WindowsFormsHost`
- **PowerShell**: PowerShell window embedding / process hosting with `WindowsFormsHost`
- **VNC**: TigerVNC window embedding / process hosting with `WindowsFormsHost`

## Tips for AI Agents

1. **Respect MVVM**: Keep business logic in ViewModels and Models, not in code-behind
2. **Use MahApps Controls**: Prefer Metro controls over standard WPF controls
3. **Localization**: Always use resource strings, never hardcode text
4. **Follow Existing Patterns**: Study similar features before implementing new ones
5. **Update Documentation**: Changes to features should include doc updates
6. **Test on Real Networks**: Network tools require real network testing
7. **Version Management**: Assembly version is auto-generated in CI/CD
8. **Submodules**: Remember to init/update submodules (Dragablz in `Source/3rdparty/`)
9. **DO NOT Modify 3rdparty**: The `Source/3rdparty/` directory contains the Dragablz submodule and is maintained manually — AI agents MUST NOT modify this directory
10. **Automation Scripts**: Use scripts in `Scripts/` directory for updating lists (OUI, ports, WHOIS servers)
11. **Check CI Status**: Both AppVeyor (main build) and GitHub Actions (CodeQL, website) must pass
12. **Dependency Updates**: Dependabot automatically creates PRs for dependency updates

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
