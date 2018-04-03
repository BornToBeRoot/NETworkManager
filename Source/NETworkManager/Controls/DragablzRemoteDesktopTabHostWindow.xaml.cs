using Dragablz;
using MahApps.Metro.Controls;

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
    }
}