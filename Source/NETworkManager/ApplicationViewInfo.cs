namespace NETworkManager
{
    public class ApplicationViewInfo
    {
        public ApplicationViewManager.Name Name { get; set; }
        public bool IsVisible { get; set; }

        public ApplicationViewInfo()
        {

        }

        public ApplicationViewInfo(ApplicationViewManager.Name name)
        {
            Name = name;
            IsVisible = true;
        }
    }
}