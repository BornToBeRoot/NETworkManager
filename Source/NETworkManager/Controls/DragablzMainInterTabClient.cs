using Dragablz;
using System.Windows;
using NETworkManager.Views.Common;

namespace NETworkManager.Controls
{
    public class DragablzMainInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            DragablzTabHostWindow dragablzTabHostWindow = new DragablzTabHostWindow();
            return new NewTabHost<DragablzTabHostWindow>(dragablzTabHostWindow, dragablzTabHostWindow.TabsContainer);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.DoNothing;
            //throw new NotImplementedException();
        }
    }
}
