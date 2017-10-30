namespace NETworkManager.Controls
{
    public class DragablzTabContent
    {
        public string Header { get; set; }
        public object Content { get; set; }

        public DragablzTabContent(string header, object content)
        {
            Header = header;
            Content = content;
        }
    }
}
