using System.ComponentModel;
using System.Windows.Input;

namespace NETworkManager.Profiles;

public interface IProfileManager : IProfileManagerMinimal
{
    ICollectionView Profiles { get; }       
    ICommand AddProfileCommand { get; }
    ICommand EditProfileCommand { get; }
    ICommand CopyAsProfileCommand { get; }
    ICommand DeleteProfileCommand { get; }
    ICommand EditGroupCommand { get; }
}
