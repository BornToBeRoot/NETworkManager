using System.ComponentModel;
using System.Windows.Input;

namespace NETworkManager.Profiles;

/// <summary>
/// Interface for a full profile manager view model, extending <see cref="IProfileManagerMinimal"/>
/// with profile collection access and profile/group management commands.
/// </summary>
public interface IProfileManager : IProfileManagerMinimal
{
    /// <summary>
    /// Gets the filtered and sorted view of the profiles collection.
    /// </summary>
    ICollectionView Profiles { get; }

    /// <summary>
    /// Gets the command to add a new profile.
    /// </summary>
    ICommand AddProfileCommand { get; }

    /// <summary>
    /// Gets the command to edit the selected profile.
    /// </summary>
    ICommand EditProfileCommand { get; }

    /// <summary>
    /// Gets the command to create a copy of the selected profile as a new profile.
    /// </summary>
    ICommand CopyAsProfileCommand { get; }

    /// <summary>
    /// Gets the command to delete the selected profile.
    /// </summary>
    ICommand DeleteProfileCommand { get; }

    /// <summary>
    /// Gets the command to edit the group of the selected profile.
    /// </summary>
    ICommand EditGroupCommand { get; }
}