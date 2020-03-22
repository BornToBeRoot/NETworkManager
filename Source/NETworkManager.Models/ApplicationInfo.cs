namespace NETworkManager.Models
{
    public class ApplicationInfo
    {
        public Application.Name Name { get; set; }
        public bool IsVisible { get; set; }

        public ApplicationInfo()
        {

        }

        public ApplicationInfo(Application.Name name)
        {
            Name = name;
            IsVisible = true;
        }

        public bool Equals(ApplicationInfo info)
        {
            if (info == null)
                return false;

            return Name == info.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals(obj as ApplicationInfo);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
