using System;

namespace NETworkManager.Models.Settings
{
    public static class CommandLineManager
    {
        public const string ParameterIdentifier = "--";
        public const char ValueSplitIdentifier = ':';

        // Public 
        public static string ParameterHelp => $"{ParameterIdentifier}help";
        public static string ParameterResetSettings => $"{ParameterIdentifier}reset-settings";
        public static string ParameterApplication => $"{ParameterIdentifier}application";

        // Internal use only
        public static string ParameterAutostart => $"{ParameterIdentifier}autostart";
        public static string ParameterRestartPid => $"{ParameterIdentifier}restart-pid";

        public static CommandLineInfo Current { get; set; }
                
        static CommandLineManager()
        {
            Current = new CommandLineInfo();

            // Detect start parameters
            var parameters = Environment.GetCommandLineArgs();

            for (var i = 1; i < parameters.Length; i++)
            {
                var param = parameters[i].Split(ValueSplitIdentifier);

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
                        if (int.TryParse(param[1], out int restartPid))
                        {
                            Current.RestartPid = restartPid;
                        }
                        else
                        {
                            WrongParameterDetected(parameters);
                            return;
                        }
                    } // Application
                    else if (param[0].Equals(ParameterApplication, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (System.Enum.TryParse(param[1], out ApplicationViewManager.Name application))
                        {
                            Current.Application = application;
                        }
                        else
                        {
                            WrongParameterDetected(parameters);
                            return;
                        }
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
                    if (i == 0)
                        continue;

                    WrongParameterDetected(parameters);
                    return;
                }
            }
        }

        public static string GetParameterWithSplitIdentifier(string parameter)
        {
            return $"{parameter}{ValueSplitIdentifier}";
        }

        private static void WrongParameterDetected(string[] parameters)
        {
            Current.WrongParameter = string.Join(" ", parameters);
            Current.Help = true;
        }
    }
}
