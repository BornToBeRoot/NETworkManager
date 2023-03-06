using System.Windows;

namespace NETworkManager.Validators;

public class ServerDependencyObjectWrapper : DependencyObject
{
    public static readonly DependencyProperty AllowOnlyIPAddressProperty = DependencyProperty.Register("AllowOnlyIPAddress",
        typeof(bool),
        typeof(ServerDependencyObjectWrapper));

    public bool AllowOnlyIPAddress
    {
        get { return (bool)GetValue(AllowOnlyIPAddressProperty); }
        set { SetValue(AllowOnlyIPAddressProperty, value); }
    }
}
