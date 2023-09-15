using NETworkManager.Utilities;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public abstract class ViewModelBase : PropertyChangedBase
{
    public ICommand CopyDataToClipboardCommand => new RelayCommand(CopyDataToClipboardAction);
    
    private static void CopyDataToClipboardAction(object data)
    {
        ClipboardHelper.SetClipboard(data.ToString());
    }
}
