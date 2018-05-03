using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzDNSLookupInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzDNSLookupTabHostWindow dragablzDNSLookupTabHostWindow = new DragablzDNSLookupTabHostWindow();
            return new NewTabHost<DragablzDNSLookupTabHostWindow>(dragablzDNSLookupTabHostWindow, dragablzDNSLookupTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
