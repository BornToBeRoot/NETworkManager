using System.Windows;
using Dragablz;
using NETworkManager.Models;

namespace NETworkManager.Controls;

public class DragablzInterTabClient(ApplicationName applicationName) : IInterTabClient
{
    public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
    {
        var dragablzTabHostWindow = new DragablzTabHostWindow(applicationName);
        return new NewTabHost<DragablzTabHostWindow>(dragablzTabHostWindow, dragablzTabHostWindow.TabsContainer);
    }

    public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
    {
        return window is MainWindow
            ? TabEmptiedResponse.CloseLayoutBranch
            : TabEmptiedResponse.CloseWindowOrLayoutBranch;
    }
}