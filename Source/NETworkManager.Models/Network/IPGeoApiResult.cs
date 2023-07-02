﻿namespace NETworkManager.Models.Network
{
   
    public class IPGeoApiResult
    {
        public IPGeoApiInfo Info { get; set; }

        /// <summary>
        /// Indicates if the IP information retrieval has failed.
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Error message if the IP information retrieval has failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        
        public IPGeoApiResult(IPGeoApiInfo info)
        {
            Info = info;         
        }

        public IPGeoApiResult(bool hasError, string errorMessage)
        {
            HasError = hasError;
            ErrorMessage = errorMessage;
        }
    }
}