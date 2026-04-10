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
    /// Gets or sets a value indicating whether to display the wrong parameter message.
    /// </summary>
    public bool DisplayWrongParameter
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the wrong parameter.
    /// </summary>
    public string WrongParameter
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the help parameter.
    /// </summary>
    public string ParameterHelp
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the reset settings parameter.
    /// </summary>
    public string ParameterResetSettings
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the application parameter.
    /// </summary>
    public string ParameterApplication
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the available application parameter values.
    /// </summary>
    public string ParameterApplicationValues
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
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