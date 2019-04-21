using System.ComponentModel;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public interface IProfileViewModel
    {
        ICollectionView Profiles { get; }
        void RefreshProfiles();
        ICommand AddProfileCommand { get; }
        ICommand EditProfileCommand { get; }
        ICommand CopyAsProfileCommand { get; }
        ICommand DeleteProfileCommand { get; }
        ICommand EditGroupCommand { get; }
    }
}
