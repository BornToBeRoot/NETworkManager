using System;

namespace NETworkManager
{
    public class ApplicationViewInfo : IEquatable<ApplicationViewInfo>
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

        public bool Equals(ApplicationViewInfo info)
        {
            if (info == null)
                return false;

            return Name == info.Name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals(obj as ApplicationViewInfo);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}