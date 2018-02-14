using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzRemoteDesktopInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzRemoteDesktopTabHostWindow dragablzRemoteDesktopTabHostWindow = new DragablzRemoteDesktopTabHostWindow();
            return new NewTabHost<DragablzRemoteDesktopTabHostWindow>(dragablzRemoteDesktopTabHostWindow, dragablzRemoteDesktopTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
