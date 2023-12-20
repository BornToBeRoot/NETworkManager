using System;

namespace NETworkManager.Utilities;

public static class SNMPOIDHelper
{
    /// <summary>
    /// Compare two OIDs.
    /// </summary>
    /// <param name="x">First OID</param>
    /// <param name="y">Second OID</param>
    /// <returns>0 if the OIDs are equal, otherwise a negative or positive value.</returns>
    public static int CompareOIDs(string x, string y)
    {
        var xArray = Array.ConvertAll(x.Split('.'), int.Parse);
        var yArray = Array.ConvertAll(y.Split('.'), int.Parse);
        
        // Compare each element of the OIDs and return the first non-equal element.
        for (var i = 0; i < Math.Min(xArray.Length, yArray.Length); i++)
        {
            if(xArray[i] != yArray[i])
                return xArray[i] - yArray[i];
        }

        // If the OIDs are equal up to this point, the shorter OID is smaller.
        return xArray.Length - yArray.Length;
    }
}