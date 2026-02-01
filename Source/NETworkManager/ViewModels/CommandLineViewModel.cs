using System;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Documentation;
using NETworkManager.Models;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the command line view.
/// </summary>
public class CommandLineViewModel : ViewModelBase
{
    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineViewModel"/> class.
    /// </summary>
    public CommandLineViewModel()
    {
        if (!string.IsNullOrEmpty(CommandLineManager.Current.WrongParameter))
        {
            WrongParameter = CommandLineManager.Current.WrongParameter;
            DisplayWrongParameter = true;
        }

        ParameterHelp = CommandLineManager.ParameterHelp;
        ParameterResetSettings = CommandLineManager.ParameterResetSettings;
        ParameterApplication =
            CommandLineManager.GetParameterWithSplitIdentifier(CommandLineManager.ParameterApplication);
        ParameterApplicationValues = string.Join(", ",
            Enum.GetValues(typeof(ApplicationName)).Cast<ApplicationName>().ToList());
    }

    #endregion

    #region Variables

    /// <summary>
    /// Backing field for <see cref="DisplayWrongParameter"/>.
    /// </summary>
    private bool _displayWrongParameter;

    /// <summary>
    /// Gets or sets a value indicating whether to display the wrong parameter message.
    /// </summary>
    public bool DisplayWrongParameter
    {
        get => _displayWrongParameter;
        set
        {
            if (value == _displayWrongParameter)
                return;

            _displayWrongParameter = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="WrongParameter"/>.
    /// </summary>
    private string _wrongParameter;

    /// <summary>
    /// Gets or sets the wrong parameter.
    /// </summary>
    public string WrongParameter
    {
        get => _wrongParameter;
        set
        {
            if (value == _wrongParameter)
                return;

            _wrongParameter = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ParameterHelp"/>.
    /// </summary>
    private string _parameterHelp;

    /// <summary>
    /// Gets or sets the help parameter.
    /// </summary>
    public string ParameterHelp
    {
        get => _parameterHelp;
        set
        {
            if (value == _parameterHelp)
                return;

            _parameterHelp = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ParameterResetSettings"/>.
    /// </summary>
    private string _parameterResetSettings;

    /// <summary>
    /// Gets or sets the reset settings parameter.
    /// </summary>
    public string ParameterResetSettings
    {
        get => _parameterResetSettings;
        set
        {
            if (value == _parameterResetSettings)
                return;

            _parameterResetSettings = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ParameterApplication"/>.
    /// </summary>
    private string _parameterApplication;

    /// <summary>
    /// Gets or sets the application parameter.
    /// </summary>
    public string ParameterApplication
    {
        get => _parameterApplication;
        set
        {
            if (value == _parameterApplication)
                return;

            _parameterApplication = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ParameterApplicationValues"/>.
    /// </summary>
    private string _parameterApplicationValues;

    /// <summary>
    /// Gets or sets the available application parameter values.
    /// </summary>
    public string ParameterApplicationValues
    {
        get => _parameterApplicationValues;
        set
        {
            if (value == _parameterApplicationValues)
                return;

            _parameterApplicationValues = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to open the documentation.
    /// </summary>
    public ICommand OpenDocumentationCommand
    {
        get { return new RelayCommand(_ => OpenDocumentationAction()); }
    }

    /// <summary>
    /// Action to open the documentation.
    /// </summary>
    private void OpenDocumentationAction()
    {
        DocumentationManager.OpenDocumentation(DocumentationIdentifier.CommandLineArguments);
    }

    #endregion
}