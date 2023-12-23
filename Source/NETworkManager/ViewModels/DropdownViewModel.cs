using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class DropdownViewModel : ViewModelBase
{
    private readonly string _valueDescription;

    private readonly List<string> _values;

    private string _selectedValue;

    public DropdownViewModel(Action<DropdownViewModel> okCommand, Action<DropdownViewModel> cancelHandler,
        List<string> values, string valueDescription)
    {
        ValueDescription = valueDescription;
        Values = values;

        SelectedValue = Values.FirstOrDefault();

        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));
    }

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }

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