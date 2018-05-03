using Dragablz;
using MahApps.Metro.Controls;
using NETworkManager.Views;
using System.Windows.Input;

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

        #region Window helper
        // Move the window when the user hold the title...
        private void HeaderBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        #endregion 
    }
}