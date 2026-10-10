using System.Windows;
using NETworkManager.Models.RDAP;

namespace NETworkManager.Validators;

public class RDAPQueryDependencyObjectWrapper : DependencyObject
{
    public static readonly DependencyProperty QueryTypeProperty = DependencyProperty.Register("QueryType",
        typeof(RDAPQueryType),
        typeof(RDAPQueryDependencyObjectWrapper));

    public RDAPQueryType QueryType
    {
        get => (RDAPQueryType)GetValue(QueryTypeProperty);
        set => SetValue(QueryTypeProperty, value);
    }
}
