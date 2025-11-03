using System;
using System.Collections.Generic;
using System.Linq;

namespace NETworkManager.ViewModels;

public class DropdownViewModel : DialogViewModelBase<DropdownViewModel>
{
    private readonly string _valueDescription;

    private readonly List<string> _values;

    private string _selectedValue;

    public DropdownViewModel(Action<DropdownViewModel> okCommand, Action<DropdownViewModel> cancelHandler,
        List<string> values, string valueDescription)
        : base(okCommand, cancelHandler)
    {
        ValueDescription = valueDescription;
        Values = values;

        SelectedValue = Values.FirstOrDefault();
    }

    public string ValueDescription
    {
        get => _valueDescription;
        private init => SetProperty(ref _valueDescription, value);
    }

    public List<string> Values
    {
        get => _values;
        private init => SetProperty(ref _values, value);
    }

    public string SelectedValue
    {
        get => _selectedValue;
        set => SetProperty(ref _selectedValue, value);
    }
}