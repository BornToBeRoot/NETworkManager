using Dragablz;
using MahApps.Metro.Controls;

namespace NETworkManager.Controls
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class DragablzPuTTYTabHostWindow : MetroWindow
    {       
        public DragablzPuTTYTabHostWindow()
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
            ((args.DragablzItem.Content as DragablzPuTTYTabItem).View as PuTTYControl).OnClose();
        }
        #endregion
    }
}