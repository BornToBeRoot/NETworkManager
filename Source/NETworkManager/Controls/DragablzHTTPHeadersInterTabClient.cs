using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzHTTPHeadersInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzHTTPHeadersTabHostWindow dragablzHTTPHeadersTabHostWindow = new DragablzHTTPHeadersTabHostWindow();
            return new NewTabHost<DragablzHTTPHeadersTabHostWindow>(dragablzHTTPHeadersTabHostWindow, dragablzHTTPHeadersTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
