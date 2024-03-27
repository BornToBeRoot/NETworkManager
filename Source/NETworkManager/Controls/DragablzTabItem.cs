using System;
using System.Windows.Controls;
using NETworkManager.ViewModels;

namespace NETworkManager.Controls;

public class DragablzTabItem : ViewModelBase
{
    private string _header;

    public DragablzTabItem(string header, UserControl view)
    {
        Header = header;
        View = view;
    }

    public DragablzTabItem(string header, UserControl view, Guid id)
    {
        Header = header;
        View = view;
        Id = id;
    }

    public string Header
    {
        get => _header;
        set
        {
            if (value == _header)
                return;

            _header = value;
            OnPropertyChanged();
        }
    }

    public UserControl View { get; set; }
    
    public Guid Id { get; set; }
}