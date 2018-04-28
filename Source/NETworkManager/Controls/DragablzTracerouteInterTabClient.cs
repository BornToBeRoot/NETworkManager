using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzTracerouteInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzTracerouteTabHostWindow dragablzTracerouteTabHostWindow = new DragablzTracerouteTabHostWindow();
            return new NewTabHost<DragablzTracerouteTabHostWindow>(dragablzTracerouteTabHostWindow, dragablzTracerouteTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
