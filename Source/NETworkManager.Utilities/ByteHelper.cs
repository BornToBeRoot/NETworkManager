using System;

namespace NETworkManager.Utilities;

public static class ByteHelper
{
    /// <summary>
    /// Compare two byte arrays.
    /// </summary>
    /// <param name="x">Byte array to compare.</param>
    /// <param name="y">Byte array to compare.</param>
    /// <returns>0 if the byte arrays are equal, otherwise a negative or positive value.</returns>
    public static int Compare(byte[] x, byte[] y)
    {
        for (var i = 0; i < Math.Min(x.Length, y.Length); i++)
        {
            if (x[i] != y[i])
                return x[i] - y[i];
        }
        
        return x.Length - y.Length;
    }
}