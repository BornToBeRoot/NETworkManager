using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzPortScannerInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzPortScannerTabHostWindow dragablzPortScannerTabHostWindow = new DragablzPortScannerTabHostWindow();
            return new NewTabHost<DragablzPortScannerTabHostWindow>(dragablzPortScannerTabHostWindow, dragablzPortScannerTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
