using NETworkManager.Models.Network;
using System.Windows;

namespace NETworkManager.Validators;

public class SNMPOIDDependencyObjectWrapper : DependencyObject
{
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode",
        typeof(SNMPMode),
        typeof(SNMPOIDDependencyObjectWrapper));

    public SNMPMode Mode
    {
        get { return (SNMPMode)GetValue(ModeProperty); }
        set { SetValue(ModeProperty, value); }
    }
}
