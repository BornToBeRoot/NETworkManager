namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class provides static methods to convert file size.
    /// </summary>
    public static class FileSizeConverter
    {
        /// <summary>
        /// Returns the human-readable file size for an arbitrary, 64-bit file size.
        /// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB".
        /// See also: https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        /// </summary>
        /// <param name="i">File size as <see cref="long"/> to convert.</param>
        /// <returns>File size as human readable <see cref="string"/>.</returns>
        public static string GetBytesReadable(long i)
        {
            // Get absolute value
            var absolute_i = (i < 0 ? -i : i);

            // Determine the suffix and readable value
            string suffix;
            double readable;

            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }

            // Divide by 1024 to get fractional value
            readable = (readable / 1024);

            // Return formatted number with suffix
            return readable.ToString("0.## ") + suffix;
        }

        /// <summary>
        /// Method to convert speed as bytes to human readable string.
        /// </summary>
        /// <param name="i">Bytes as <see cref="long"/>.</param>
        /// <param name="suffix">Suffix which is appended to the converted bytes.</param>
        /// <returns>Speed as human readable <see cref="string"/>.</returns>
        public static string GetBytesSpeedReadable(long i, string suffix)
        {
            return GetBytesReadable(i) + suffix;
        }
    }
}
