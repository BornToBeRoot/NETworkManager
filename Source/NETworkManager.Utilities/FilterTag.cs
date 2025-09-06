namespace NETworkManager.Utilities;

public class FilterTag : PropertyChangedBase
{
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string Name { get; set; }

    public FilterTag(bool isSelected, string name)
    {
        IsSelected = isSelected;
        Name = name;
    }
}
