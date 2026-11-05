namespace NETworkManager.Controls;

/// <summary>
///     Override of the SNMP version and credentials in the profile and group dialog (SNMP tool, IP Scanner), the
///     DataContext is a <see cref="ViewModels.SNMPCredentialsViewModel" />.
/// </summary>
public partial class SNMPCredentialsControl
{
    public SNMPCredentialsControl()
    {
        InitializeComponent();

        // Invalid input of the text boxes: the dialog can't be saved
        InputErrorTracker.Track(this);
    }
}
