using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzPuTTYInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzPuTTYTabHostWindow dragablzPuTTYTabHostWindow = new DragablzPuTTYTabHostWindow();
            return new NewTabHost<DragablzPuTTYTabHostWindow>(dragablzPuTTYTabHostWindow, dragablzPuTTYTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
