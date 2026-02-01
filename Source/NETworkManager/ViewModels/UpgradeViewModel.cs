using NETworkManager.Documentation;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class UpgradeViewModel : ViewModelBase
{
    public static string Title => string.Format(Localization.Resources.Strings.UpgradedToXXX, AssemblyManager.Current.Version);

    public UpgradeViewModel(Action<UpgradeViewModel> continueCommand)
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
