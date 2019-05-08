using System.ComponentModel;
using System.Windows.Input;

namespace NETworkManager.Models.Settings
{
    public interface IProfileManager
    {
        ICollectionView Profiles { get; }
        void RefreshProfiles();
        void OnProfileDialogOpen();
        void OnProfileDialogClose();
        ICommand AddProfileCommand { get; }
        ICommand EditProfileCommand { get; }
        ICommand CopyAsProfileCommand { get; }
        ICommand DeleteProfileCommand { get; }
        ICommand EditGroupCommand { get; }
    }
}
