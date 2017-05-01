using System;

namespace NETworkManager.Settings
{
    public static class CommandLine
    {
        public static CommandLineInfo Current { get; set; }

        /// <summary>
        /// Get the arguments passed by the command line
        /// </summary>
        /// <returns>CommandLineInfo</returns>
        public static void Parse()
        {
            Current = new CommandLineInfo();

            // Get the command line args
            string[] args = Environment.GetCommandLineArgs();

            // Detect start parameters
            foreach (string arg in args)
            {
                if (arg.StartsWith("--"))
                {
                    string argument = arg.ToLower().TrimStart('-');

                    // Autostart
                    if (string.Equals(argument, Properties.Resources.StartParameter_Autostart, StringComparison.OrdinalIgnoreCase))
                        Current.Autostart = true;
                }
            }
        }
    }
}
