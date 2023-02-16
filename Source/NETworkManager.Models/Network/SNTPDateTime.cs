using System;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class is used to store informations about SNTP date time.    
    /// </summary>
    public class SNTPDateTime
    {
        /// <summary>
        /// Time when the requests started.
        /// </summary>
        public DateTime LocalStartTime { get; set; }

        /// <summary>
        /// Time from the SNTP server.
        /// </summary>
        public DateTime NetworkTime { get; set; }

        /// <summary>
        /// Time when the requests ended.
        /// </summary>
        public DateTime LocalEndTime { get; set; }

        /// <summary>
        /// Round trip delay in milliseconds.
        /// </summary>
        public double RoundTripDelay { get; set; }

        /// <summary>
        /// Offset to the local clock in seconds.
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// Create an instance of <see cref="SNTPDateTime"/>.
        /// </summary>
        public SNTPDateTime()
        {

        }
    }
}
