using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzSNMPInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzSNMPTabHostWindow dragablzSNMPTabHostWindow = new DragablzSNMPTabHostWindow();
            return new NewTabHost<DragablzSNMPTabHostWindow>(dragablzSNMPTabHostWindow, dragablzSNMPTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
