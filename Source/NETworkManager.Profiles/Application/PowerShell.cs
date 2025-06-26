﻿using NETworkManager.Models.PowerShell;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application;

public class PowerShell
{
    public static PowerShellSessionInfo CreateSessionInfo(ProfileInfo profile)
    {
        // Get group info
        var group = ProfileManager.GetGroupByName(profile.Group);

        return new PowerShellSessionInfo
        {
            EnableRemoteConsole = profile.PowerShell_EnableRemoteConsole,
            Host = profile.PowerShell_Host,

            Command = profile.PowerShell_OverrideCommand
                ? profile.PowerShell_Command
                : group.PowerShell_OverrideCommand
                    ? group.PowerShell_Command
                    : SettingsManager.Current.PowerShell_Command,
            AdditionalCommandLine = profile.PowerShell_OverrideAdditionalCommandLine
                ? profile.PowerShell_AdditionalCommandLine
                : group.PowerShell_OverrideAdditionalCommandLine
                    ? group.PowerShell_AdditionalCommandLine
                    : SettingsManager.Current.PowerShell_AdditionalCommandLine,
            ExecutionPolicy = profile.PowerShell_OverrideExecutionPolicy
                ? profile.PowerShell_ExecutionPolicy
                : group.PowerShell_OverrideExecutionPolicy
                    ? group.PowerShell_ExecutionPolicy
                    : SettingsManager.Current.PowerShell_ExecutionPolicy
        };
    }
}