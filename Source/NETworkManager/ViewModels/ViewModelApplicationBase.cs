using System.Windows.Input;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public abstract class ViewModelApplicationBase : ViewModelBase
{
    public ICommand CopyDataToClipboardCommand => new RelayCommand(CopyDataToClipboardAction);
    
    private static void CopyDataToClipboardAction(object data)
    {
        ClipboardHelper.SetClipboard(data.ToString());
    }
}
