---
layout: default
title: General
parent: Settings
grand_parent: Documentation
nav_order: 1
description: "Documentation of the general settings"
permalink: /Documentation/Settings/General
---

# General

### Show the following application on startup:

Default application that is displayed when the application is launched.

Type: `NETworkManager.Models.ApplicationName`

Default: `Dashboard`

### Visible applications

Applications that are displayed in the bar.

Type: `NETworkManager.Models.ApplicationName`

Default: `All`

### Hidden applications

Applications that are not displayed in the bar.

Type: `NETworkManager.Models.ApplicationName`

Default: `None`

### Run background job every x-minutes

Run a background job every x-minutes to save profiles and settings.

Type: `Integer`

Default: `5` [Min `0`, Max `120`]

{: .note }
The value 0 will disable the background job. Changes to this value will take effect after restarting the application.

### Number of stored entries

Maximum number of entries stored in the history for several application inputs.

Type: `Integer`

Default: `5` [Min `0`, Max `25`]

### ThreadPool additional min. threads

Additional [minimum number of threads](https://learn.microsoft.com/en-us/dotnet/api/system.threading.threadpool.setminthreads?view=net-7.0){:target="\_blank"} of the applications [ThreadPool](https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool){:target="\_blank"} that are created on demand, as new requests are made, before switching to an algorithm for managing thread creation and destruction. This can improve e.g. the IP scanner or port scanner. The value is added to the default settings.

Type: `Integer`

Default: `512` [Min `0`, Max `1024`]

{: .note}
The value 0 leaves the default settings (number of CPU threads). If the value is to high, performance problems may occur. If the value is higher than the max. threads of the ThreadPool, the max. threads will be used. Changes to this value will take effect after restarting the application. Wheter the value was set successfully can be seen in the log file under `%LocalAppData%\NETworkManager\NETworkManager.log`.
