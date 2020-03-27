using Dragablz;
using NETworkManager.Models;
using System.Windows;

namespace NETworkManager.Controls
{
    public class DragablzInterTabClient : IInterTabClient
    {
        private readonly ApplicationName _applicationName;

        public DragablzInterTabClient(ApplicationName applicationName)
        {
            _applicationName = applicationName;
        }

        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            var dragablzTabHostWindow = new DragablzTabHostWindow(_applicationName);
            return new NewTabHost<DragablzTabHostWindow>(dragablzTabHostWindow, dragablzTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return window is MainWindow ? TabEmptiedResponse.DoNothing : TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }
    }
}
