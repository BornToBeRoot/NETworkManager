using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class DropdownViewModel : ViewModelBase
    {
        public ICommand OKCommand { get; }

        public ICommand CancelCommand { get; }

        private string _valueDescription;
        public string ValueDescription
        {
            get => _valueDescription;
            set
            {
                if(value == _valueDescription)
                    return;

                _valueDescription = value;
                OnPropertyChanged();
            }
        }

        private List<string> _values;
        public List<string> Values
        {
            get => _values;
            set
            {
                if (value == _values)
                    return;

                _values = value;
                OnPropertyChanged();
            }
        }

        private string _selectedValue;
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

        public DropdownViewModel(Action<DropdownViewModel> okCommand, Action<DropdownViewModel> cancelHandler, List<string> values, string valueDescription)
        {
            ValueDescription = valueDescription;
            Values = values;

            SelectedValue = Values.FirstOrDefault();

            OKCommand = new RelayCommand(p => okCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));
        }      
    }
}