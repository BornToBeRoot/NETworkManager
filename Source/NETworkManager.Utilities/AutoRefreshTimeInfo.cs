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
        /// Create an empty <see cref="AutoRefreshTimeInfo"/>.
        /// </summary>
        public AutoRefreshTimeInfo()
        {

        }

        /// <summary>
        /// Create an <see cref="AutoRefreshTimeInfo"/> with values.
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