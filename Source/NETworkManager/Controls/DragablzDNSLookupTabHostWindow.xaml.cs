using Dragablz;
using MahApps.Metro.Controls;
using NETworkManager.Views;

namespace NETworkManager.Controls
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class DragablzDNSLookupTabHostWindow : MetroWindow
    {        
        public DragablzDNSLookupTabHostWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region ICommand & Actions
        public ItemActionCallback CloseItemCommand
        {
            get { return CloseItemAction; }
        }

        private void CloseItemAction(ItemActionCallbackArgs<TabablzControl> args)
        {
            ((args.DragablzItem.Content as DragablzDNSLookupTabItem).View as DNSLookupView).CloseTab();
        }
        #endregion
    }
}