using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzPingInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzPingTabHostWindow dragablzPingTabHostWindow = new DragablzPingTabHostWindow();
            return new NewTabHost<DragablzPingTabHostWindow>(dragablzPingTabHostWindow, dragablzPingTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
