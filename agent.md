# AI Assistant Guidelines for NETworkManager

This document provides guidelines for AI assistants (GitHub Copilot, Claude, Junie, Cursor, etc.) working with the NETworkManager codebase.

## Project Overview

**NETworkManager** is a powerful network management and troubleshooting application for Windows, built with C# and WPF using the MVVM architecture pattern.

- **Repository**: https://github.com/BornToBeRoot/NETworkManager
- **Documentation**: https://borntoberoot.net/NETworkManager
- **License**: GNU General Public License v3

## Technology Stack

- **Language**: C# (Latest version features used, e.g., file-scoped namespaces, target-typed new)
- **Framework**: .NET 10.0 (Windows)
- **Architecture**: MVVM (Model-View-ViewModel)
- **UI Framework**: WPF (using MahApps.Metro and Dragablz)

## Getting Started

### Prerequisites

- [.NET 10.x SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 with `.NET desktop development` and `Universal Windows Platform development` workloads
- Windows 10/11 (required for WPF development)

### Cloning the Repository

```bash
git clone https://github.com/BornToBeRoot/NETworkManager
cd NETworkManager
git submodule update --init
```

### Building the Project

```bash
# Restore dependencies
dotnet restore ./Source/NETworkManager.sln

# Build the solution
dotnet build ./Source/NETworkManager.sln --configuration Release --no-restore
```

Alternatively, open `Source/NETworkManager.sln` in Visual Studio or JetBrains Rider.

## Coding Conventions

### Naming Conventions

- **Classes, Methods, Properties, Enums**: PascalCase
- **Private Fields**: `_camelCase` (underscore prefix)
  - Exception: `private static readonly ILog Log` (PascalCase)
- **Event Handlers**: `Object_EventName` (e.g., `SettingsManager_PropertyChanged`)
- **Interfaces**: `IPascalCase` (prefix with I)

### Formatting

- **Indentation**: 4 spaces
- **Braces**: Allman style (opening brace on a new line)
- **Namespaces**: File-scoped namespaces (`namespace MyNamespace;`)
- **Imports**: `using` directives sorted alphabetically, `System` namespaces mixed in
- **Grouping**: `#region` used to group related members (e.g., Variables, Events, Properties)

### Specific Practices

- **Logging**: Use `log4net` via `LogManager.GetLogger(typeof(ClassName))`
- **Properties**: Implement `INotifyPropertyChanged` (usually via `ViewModelBase`). Use expression-bodied members for getters where concise.
- **Null Checks**: Use pattern matching or standard checks
- **Localization**: Use static `Strings` class (e.g., `localization:Strings.MyString`)
- **Documentation**: XML documentation comments (`///`) for all public members

## Code Organization and Package Structure

### Projects

The solution is divided into multiple projects to separate concerns:

| Project | Description |
|---------|-------------|
| **NETworkManager** | Main WPF application. Contains entry point (`App.xaml`), main window, and feature-specific Views and ViewModels |
| **NETworkManager.Controls** | Custom WPF UI controls used throughout the application |
| **NETworkManager.Converters** | WPF Value Converters for data binding |
| **NETworkManager.Models** | Core business logic, data entities, and service wrappers (e.g., Network, Ping, Traceroute logic) |
| **NETworkManager.Utilities** | General purpose utility classes and helpers |
| **NETworkManager.Utilities.WPF** | WPF-specific utility classes |
| **NETworkManager.Settings** | Application settings and configuration persistence |
| **NETworkManager.Profiles** | Network profile handling logic |
| **NETworkManager.Localization** | Localization resources (`Strings.resx`) |
| **NETworkManager.Validators** | Input validation logic |
| **NETworkManager.Documentation** | Application documentation resources |
| **NETworkManager.Update** | Application update checking and installation logic |
| **NETworkManager.Setup** | Installer setup (WiX) |

### Directory Structure

```
/Source
├── /NETworkManager          # Main application
│   ├── /Views              # XAML Views (UserControls, Windows)
│   ├── /ViewModels         # ViewModels corresponding to Views
│   └── /Resources          # App-specific resources
├── /NETworkManager.Models
│   └── Organized by feature (e.g., /Network, /Ping, /Lookup)
├── /3rdparty               # Third-party libraries and dependencies
└── ...other projects
```

## Important Guidelines

### Third-Party Libraries

**AI assistants must not modify libraries and dependencies in the `/3rdparty` folder.** These are external dependencies managed separately.

### Localization

All user-facing strings should be localized using the `NETworkManager.Localization` project. Reference strings using `localization:Strings.MyString` in XAML or the `Strings` class in code.

### MVVM Pattern

Follow the MVVM pattern strictly:
- **Views** should contain minimal code-behind (only UI-related logic)
- **ViewModels** handle presentation logic and implement `INotifyPropertyChanged`
- **Models** contain business logic and data entities

### Configuration Files

- `Source/.editorconfig` - EditorConfig settings for consistent code style
- `Source/global.json` - .NET SDK version configuration
- `Source/NETworkManager.sln.DotSettings` - ReSharper/Rider settings

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes following the coding conventions above
4. Submit a pull request

For detailed contributing guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md).

## Additional Resources

- **Website & Documentation**: https://borntoberoot.net/NETworkManager
- **Issue Tracker**: https://github.com/BornToBeRoot/NETworkManager/issues
- **Discussions**: https://github.com/BornToBeRoot/NETworkManager/discussions
- **Translation (Transifex)**: https://app.transifex.com/BornToBeRoot/NETworkManager/dashboard/
