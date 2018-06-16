using Dragablz;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzInterTabClient : IInterTabClient
    {
        ApplicationViewManager.Name _applicationName;

        public DragablzInterTabClient(ApplicationViewManager.Name applicationName)
        {
            _applicationName = applicationName;
        }

        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzTabHostWindow dragablzTabHostWindow = new DragablzTabHostWindow(_applicationName);
            return new NewTabHost<DragablzTabHostWindow>(dragablzTabHostWindow, dragablzTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return window is MainWindow ? TabEmptiedResponse.DoNothing : TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
