using System;

namespace NETworkManager.Models.Settings
{
    public static class CommandLineManager
    {
        public const string ParameterIdentifier = "--";
        public const char ParameterSplit = ':';

        // Public + Help
        public const string ParameterHelp = ParameterIdentifier + "help";
        public const string ParameterResetSettings = ParameterIdentifier + "reset-settings";

        // Internal use only
        public const string ParameterAutostart = ParameterIdentifier + "autostart";
        public const string ParameterRestartPid = ParameterIdentifier + "restart-pid";

        public static CommandLineInfo Current { get; set; }

        /// <summary>
        /// Get the arguments passed by the command line
        /// </summary>
        /// <returns>CommandLineInfo</returns>
        public static void Parse()
        {
            Current = new CommandLineInfo();

            // Detect start parameters
            string[] parameters = Environment.GetCommandLineArgs();

            for (int i = 0; i < parameters.Length; i++)
            {
                string[] param = parameters[i].Split(ParameterSplit);

                if (param[0].StartsWith(ParameterIdentifier))
                {
                    if (param[0].Equals(ParameterHelp, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Current.Help = true;
                    }// Autostart
                    else if (param[0].Equals(ParameterAutostart, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Current.Autostart = true;
                    } // Reset Settings                    
                    else if (param[0].Equals(ParameterResetSettings, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Current.ResetSettings = true;
                    } // Restart
                    else if (param[0].Equals(ParameterRestartPid, StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(param[1], out int restartPid);

                        Current.RestartPid = restartPid;
                    }
                    else
                    {
                        WrongParameterDetected(parameters);
                        return;
                    }
                }
                else
                {
                    // Ignore the first parameter because it's the path of the .exe
                    if (i != 0)
                    {
                        WrongParameterDetected(parameters);
                        return;
                    }
                }
            }
        }

        private static void WrongParameterDetected(string[] parameters)
        {
            Current.WrongParameter = string.Join(" ", parameters);
            Current.Help = true;
        }
    }
}
