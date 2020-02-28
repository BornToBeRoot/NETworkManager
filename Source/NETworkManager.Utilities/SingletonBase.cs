namespace NETworkManager.Utilities
{
    /// <summary>
    /// Abstract class to implement a generic singleton.
    /// </summary>
    /// <typeparam name="T">Class</typeparam>
    public abstract class SingletonBase<T> where T : class, new()
    {
        /// <summary>
        /// Variable for the instance of the class. 
        /// </summary>
        private static T _instance;

        /// <summary>
        /// Method to return the current instance of the class.
        /// </summary>
        /// <returns>Instance of the class.</returns>
        public static T GetInstance()
        {
            if (_instance == null)
                _instance = new T();

            return _instance;
        }        
    }
}
