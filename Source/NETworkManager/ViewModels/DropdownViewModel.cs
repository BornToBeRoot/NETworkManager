using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for a dropdown dialog.
/// </summary>
public class DropDownViewModel : ViewModelBase
{
    /// <summary>
    /// Backing field for <see cref="ValueDescription"/>.
    /// </summary>
    private readonly string _valueDescription;

    /// <summary>
    /// Backing field for <see cref="Values"/>.
    /// </summary>
    private readonly List<string> _values;

    /// <summary>
    /// Backing field for <see cref="SelectedValue"/>.
    /// </summary>
    private string _selectedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownViewModel"/> class.
    /// </summary>
    /// <param name="okCommand">The action to execute when OK is clicked.</param>
    /// <param name="cancelHandler">The action to execute when Cancel is clicked.</param>
    /// <param name="values">The list of values to display in the dropdown.</param>
    /// <param name="valueDescription">The description of the value.</param>
    public DropDownViewModel(Action<DropDownViewModel> okCommand, Action<DropDownViewModel> cancelHandler,
        List<string> values, string valueDescription)
    {
        ValueDescription = valueDescription;
        Values = values;

        SelectedValue = Values.FirstOrDefault();

        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    /// <summary>
    /// Gets the command to confirm the selection.
    /// </summary>
    public ICommand OKCommand { get; }

    /// <summary>
    /// Gets the command to cancel the operation.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the description of the value.
    /// </summary>
    public string ValueDescription
    {
        get => _valueDescription;
        private init
        {
            if (value == _valueDescription)
                return;

            _valueDescription = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the list of values.
    /// </summary>
    public List<string> Values
    {
        get => _values;
        private init
        {
            if (value == _values)
                return;

            _values = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the selected value.
    /// </summary>
    public string SelectedValue
    {
        get => _selectedValue;
        set
        {
            if (value == _selectedValue)
                return;

            _selectedValue = value;
            OnPropertyChanged();
        }
    }
}