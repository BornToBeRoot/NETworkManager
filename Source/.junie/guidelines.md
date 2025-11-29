# Coding Conventions

Junie Pro generated these guidelines using the Gemini 3 model.

## General
*   **Language**: C# (Latest version features used, e.g., file-scoped namespaces, target-typed new).
*   **Framework**: .NET 10.0 (Windows).
*   **Architecture**: MVVM (Model-View-ViewModel).
*   **UI Framework**: WPF (using MahApps.Metro and Dragablz).

## Naming Conventions
*   **Classes, Methods, Properties, Enums**: PascalCase.
*   **Private Fields**: `_camelCase` (underscore prefix).
    *   Exception: `private static readonly ILog Log` (PascalCase).
*   **Event Handlers**: `Object_EventName` (e.g., `SettingsManager_PropertyChanged`).
*   **Interfaces**: `IPascalCase` (prefix with I).

## Formatting
*   **Indentation**: 4 spaces.
*   **Braces**: Allman style (opening brace on a new line).
*   **Namespaces**: File-scoped namespaces (`namespace MyNamespace;`).
*   **Imports**: `using` directives sorted alphabetically, `System` namespaces mixed in.
*   **Grouping**: `#region` used to group related members (e.g., Variables, Events, Properties).

## Specific Practices
*   **Logging**: Use `log4net` via `LogManager.GetLogger(typeof(ClassName))`.
*   **Properties**: Implement `INotifyPropertyChanged` (usually via `ViewModelBase`). Use expression-bodied members for getters where concise.
*   **Null Checks**: Use pattern matching or standard checks.
*   **Localization**: Use static `Strings` class (e.g., `localization:Strings.MyString`).
*   **Documentation**: XML documentation comments (`///`) for all public members.

# Code Organization and Package Structure

## Projects
The solution is divided into multiple projects to separate concerns:

*   **NETworkManager**: The main WPF application project. Contains the entry point (`App.xaml`), main window, and feature-specific Views and ViewModels.
*   **NETworkManager.Controls**: Custom WPF UI controls used throughout the application.
*   **NETworkManager.Converters**: WPF Value Converters for data binding.
*   **NETworkManager.Models**: Core business logic, data entities, and service wrappers (e.g., Network, Ping, Traceroute logic).
*   **NETworkManager.Utilities**: General purpose utility classes and helpers.
*   **NETworkManager.Utilities.WPF**: WPF-specific utility classes.
*   **NETworkManager.Settings**: Manages application settings and configuration persistence.
*   **NETworkManager.Profiles**: Logic for handling network profiles.
*   **NETworkManager.Localization**: Contains localization resources (`Strings.resx`).
*   **NETworkManager.Validators**: Input validation logic.
*   **NETworkManager.Documentation**: Application documentation resources.
*   **NETworkManager.Update**: Logic for checking and applying application updates.
*   **NETworkManager.Setup**: Related to the installer setup.

## Directory Structure
*   `/NETworkManager`
    *   `/Views`: XAML Views (UserControls, Windows).
    *   `/ViewModels`: ViewModels corresponding to Views.
    *   `/Resources`: App-specific resources.
*   `/NETworkManager.Models`
    *   Organized by feature (e.g., `/Network`, `/Ping`, `/Lookup`).
*   `/3rdparty`: Third-party libraries and dependencies. Junie must not modify these libraries and dependencies.
