using System;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// This class contains a collection of useful methods to interact with an array
    /// </summary>
    public static class ArrayHelper
    {
        /// <summary>
        /// This method creates an sub array from an array
        /// </summary>
        /// <typeparam name="T">Object array.</typeparam>
        /// <param name="data">Any array.</param>
        /// <param name="index">Start index.</param>
        /// <param name="length">Length of the new array.</param>
        /// <returns>Sub array of the array.</returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
