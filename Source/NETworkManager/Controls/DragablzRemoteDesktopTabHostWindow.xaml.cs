using Dragablz;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace NETworkManager.Controls
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class DragablzRemoteDesktopTabHostWindow : MetroWindow
    {        
        public DragablzRemoteDesktopTabHostWindow()
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
            ((args.DragablzItem.Content as DragablzRemoteDesktopTabItem).View as RemoteDesktopControl).OnClose();
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