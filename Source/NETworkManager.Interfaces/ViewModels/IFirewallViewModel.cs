using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NETworkManager.Interfaces.ViewModels;

public interface IFirewallViewModel
{
    public ObservableCollection<IFirewallRuleViewModel> FirewallRulesInterface { get; }
    
    public ICommand ExpandAllProfileGroupsCommand { get; }
    
    public ICommand CollapseAllProfileGroupsCommand { get; }
    
    public int MaxLengthHistory { get; }
    
    public static IFirewallViewModel? Instance { get; set; }

    public static void SetInstance(IFirewallViewModel? viewModel)
    {
        Instance = viewModel;
    }
}