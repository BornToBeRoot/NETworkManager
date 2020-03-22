namespace NETworkManager.Models
{
    public class ApplicationInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public Application.Name Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationInfo()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ApplicationInfo(Application.Name name)
        {
            Name = name;
            IsVisible = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool Equals(ApplicationInfo info)
        {
            if (info == null)
                return false;

            return Name == info.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
