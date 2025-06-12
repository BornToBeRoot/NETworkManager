using NETworkManager.Utilities;
using NETworkManager.Settings;
using System;
using System.Windows.Input;
using NETworkManager.Documentation;

namespace NETworkManager.ViewModels;

public class UpgradeNoteViewModel : ViewModelBase
{
    public static string Title => string.Format(Localization.Resources.Strings.UpgradedToXXX, AssemblyManager.Current.Version);
    
    public UpgradeNoteViewModel(Action<UpgradeNoteViewModel> continueCommand)
    {
        ContinueCommand = new RelayCommand(_ => continueCommand(this));
    }

    public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

    private static void OpenWebsiteAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }

    public ICommand OpenChangelogCommand => new RelayCommand(OpenChangelogAction);

    private void OpenChangelogAction(object obj)
    {
        DocumentationManager.OpenChangelog();
    }

    public ICommand ContinueCommand { get; }
}
