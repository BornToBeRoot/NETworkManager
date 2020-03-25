namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class stores auto refresh time informations.
    /// </summary>
    public class AutoRefreshTimeInfo
    {
        /// <summary>
        /// Value is combined with <see cref="TimeUnit"/>.
        /// Example: 5 Mintues, 2 Hours.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// <see cref="TimeUnit"/> is combined with <see cref="Value"/>.
        /// </summary>
        public TimeUnit TimeUnit { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRefreshTimeInfo"/> class.
        /// </summary>
        public AutoRefreshTimeInfo()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRefreshTimeInfo"/> class with parameters.
        /// </summary>
        /// <param name="value"><see cref="Value"/>.</param>
        /// <param name="timenUnit"><see cref="TimeUnit"/>.</param>
        public AutoRefreshTimeInfo(int value, TimeUnit timenUnit)
        {
            Value = value;
            TimeUnit = timenUnit;
        }
    }
}