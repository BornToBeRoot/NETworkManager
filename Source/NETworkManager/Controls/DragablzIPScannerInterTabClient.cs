using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzIPScannerInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzIPScannerTabHostWindow dragablzIPScannerTabHostWindow = new DragablzIPScannerTabHostWindow();
            return new NewTabHost<DragablzIPScannerTabHostWindow>(dragablzIPScannerTabHostWindow, dragablzIPScannerTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
        }
    }
}
