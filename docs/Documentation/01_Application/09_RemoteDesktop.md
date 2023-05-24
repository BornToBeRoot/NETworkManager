---
layout: default
title: Remote Desktop
parent: Application
grand_parent: Documentation
nav_order: 9
description: "Documentation of Remote Desktop"
permalink: /Documentation/Application/RemoteDesktop
---

# Remote Desktop

With **Remote Desktop** you can connect to a remote computer using the Remote Desktop Protocol (RDP).

{: .info}
Remote Desktop Protocol (RDP) is a proprietary protocol developed by Microsoft that allows users to remotely access and control a computer or virtual machine (VM) over a network connection. It provides a secure communication channel by encrypting the data transmitted between the client and the remote computer, protecting it from unauthorized access.

![RemoteDesktop](09_RemoteDesktop.png)

## Connect

### Host

Host to connect to via VNC.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

### Use credentials

Use credentials to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Username

Username to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Use credentials](#use-credentials) is enabled.

### Domain

Domain to authenticate with the remote computer. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Use credentials](#use-credentials) is enabled.

### Password

Password to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Use credentials](#use-credentials) is enabled.

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
If this option is enabled, the [Host](#host-1) is overwritten by the host from the general settings and the [Host](#host-1) is disabled.

### Host

Host to connect to via VNC.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

### Use credentials

Use credentials to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Username

Username to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Use credentials](#use-credentials-1) is enabled.

### Domain

Domain to authenticate with the remote computer. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Use credentials](#use-credentials-1) is enabled.

### Password

Password to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Use credentials](#use-credentials-1) is enabled.

{: .warning }
The profile file should be encrypted when storing sensitive information like passwords. See [FAQ > Profile encryption](NETworkManager/FAQ#profile-encryption) for more details.

### Display

Display setting used for the remote desktop connection.

**Type:** `Boolean` (`RadioButtons`)

**Default:** `Use the current view size as screen size`

**Possible values:**

- `Adjust screen automatically` (Remote Desktop will adjust the screen size automatically when the window is resized.)
- `Use the current view size as screen size` (Remote Desktop will use the current view size as screen size, but will not adjust the screen size automatically when the window is resized.)
- `Fixed screen size` (Remote Desktop uses the fixed screen size selected in the drop-down list below.)
  **Possible values:**
  - `640x480`
  - `800x600`
  - `1024x768`
  - `1280x720`
  - `1280x768`
  - `1280x800`
  - `1280x1024`
  - `1360x768`
  - `1440x900`
  - `1440x1050`
  - `1680x1050`
  - `1920x1080`
- `Custom screen size` (Remote Desktop uses the custom screen size that can be entered in the text fields below.)

### Color depth

Color depth used for the remote desktop connection.

**Type:** `Integer`

**Default:** `32`

**Possible values:**

- `15`
- `16`
- `24`
- `32`

### Port

Port used for the remote desktop connection.

**Type:** `Integer`

**Default:** `3389`

### Credential Security Support Provider

Use the Credential Security Support Provider (CredSSP) protocol to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Authentication level

Authentication level used for the remote desktop connection.

**Type:** `Integer`

**Default:** `2` [Min `0`, Max `3`]

**Possible values:**

- `0` (If server authentication fails, connect to the computer without warning.)
- `1` (If server authentication fails, do not establish a connection.)
- `2` (If server authentication fails, show a warning and allow me to connect or refuse the connection.)
- `3` (No authentication requirement is specified.)

### Enable gateway server

Enable the use of a gateway server for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Server name

Hostname of the gateway server used for the remote desktop connection.

**Type:** `String`

**Default:** `Empty`

**Example:** `gateway.borntoberoot.net`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled.

### Bypass for local addresses

Bypass the gateway server for local addresses.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled.

### Logon method

Logon method used for the gateway server.

**Type:** `NETworkManager.Models.RemoteDesktop.GatewayUserSelectedCredsSource`

**Default:** `Allow me to select later`

**Possible values:**

`Ask for password (NTLM)` (`Userpass`)
`Smart card or Windows Hello for Business` (`Smartcard`)
`Allow me to select later` (`Any`)

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled.

### Share gateway credentials with remote computer

Share the gateway credentials with the remote computer.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-2) is enabled.

### Use gateway credentials

Use credentials to authenticate with the gateway server.

**Type:** `Boolean`

**Default:** `Disabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled and [Logon method](#logon-method) is set to `Ask for password (NTLM)`.

### Username

Username to authenticate with the gateway server.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled, [Logon method](#logon-method) is set to `Ask for password (NTLM)` and [Use gateway credentials](#use-gateway-credentials) is enabled.

### Domain

Domain to authenticate with the gateway server. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled, [Logon method](#logon-method) is set to `Ask for password (NTLM)` and [Use gateway credentials](#use-gateway-credentials) is enabled.

### Password

Password to authenticate with the gateway server.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server) is enabled, [Logon method](#logon-method) is set to `Ask for password (NTLM)` and [Use gateway credentials](#use-gateway-credentials) is enabled.

{: .warning }
The profile file should be encrypted when storing sensitive information like passwords. See [FAQ > Profile encryption](NETworkManager/FAQ#profile-encryption) for more details.

### Remote audio playback

Configure the remote audio playback for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.AudioRedirectionMode`

**Default:** `Play on remote computer`

**Possible values:**

- `Play on this computer`
- `Play on remote computer`
- `Do not play`

### Remote audio recording

Configure the remote audio recording for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.AudioCaptureRedirectionMode`

**Default:** `Do not record`

**Possible values:**

- `Record from this computer`
- `Do not record`

### Apply Windows key combinations

Configure the Windows key combinations for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.KeyboardHookMode`

**Default:** `On the remote computer`

**Possible values:**

- `On this computer`
- `On the remote computer`

### Redirect clipboard

Configure the clipboard redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Enabled`

### Redirect devices

Configure the device redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect drives

Configure the drive redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect ports

Configure the port redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect smartcards

Configure the smartcard redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect printers

Configure the printer redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Persitent bitmap caching

Configure the persistent bitmap caching for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Reconnect if the connection is dropped

Automatically reconnect if the connection is dropped.

**Type:** `Boolean`

**Default:** `Disabled`

### Network connection type

Configure the network connection type for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.NetworkConnectionType`

**Default:** `Detect connection quality automatically`

**Possible values:**

- `Detect connection quality automatically`
- `Modem (56 kbps)`
- `Low-speed broadband (256 kbps - 2 Mbps)`
- `Satellite (2 Mbps - 16 Mbps with high latency)`
- `High-speed broadband (2 Mbps - 10 Mbps)`
- `WAN (10 Mbps or higher with high latency)`
- `LAN (10 Mbps or higher)`

## Group

### Use credentials

Use credentials to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Username

Username to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Use credentials](#use-credentials-2) is enabled.

### Domain

Domain to authenticate with the remote computer. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Use credentials](#use-credentials-2) is enabled.

### Password

Password to authenticate with the remote computer.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Use credentials](#use-credentials-2) is enabled.

{: .warning }
The profile file should be encrypted when storing sensitive information like passwords. See [FAQ > Profile encryption](NETworkManager/FAQ#profile-encryption) for more details.

### Display

Display setting used for the remote desktop connection.

**Type:** `Boolean` (`RadioButtons`)

**Default:** `Use the current view size as screen size`

**Possible values:**

- `Adjust screen automatically` (Remote Desktop will adjust the screen size automatically when the window is resized.)
- `Use the current view size as screen size` (Remote Desktop will use the current view size as screen size, but will not adjust the screen size automatically when the window is resized.)
- `Fixed screen size` (Remote Desktop uses the fixed screen size selected in the drop-down list below.)
  **Possible values:**
  - `640x480`
  - `800x600`
  - `1024x768`
  - `1280x720`
  - `1280x768`
  - `1280x800`
  - `1280x1024`
  - `1360x768`
  - `1440x900`
  - `1440x1050`
  - `1680x1050`
  - `1920x1080`
- `Custom screen size` (Remote Desktop uses the custom screen size that can be entered in the text fields below.)

### Color depth

Color depth used for the remote desktop connection.

**Type:** `Integer`

**Default:** `32`

**Possible values:**

- `15`
- `16`
- `24`
- `32`

### Port

Port used for the remote desktop connection.

**Type:** `Integer`

**Default:** `3389`

### Credential Security Support Provider

Use the Credential Security Support Provider (CredSSP) protocol to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Authentication level

Authentication level used for the remote desktop connection.

**Type:** `Integer`

**Default:** `2` [Min `0`, Max `3`]

**Possible values:**

- `0` (If server authentication fails, connect to the computer without warning.)
- `1` (If server authentication fails, do not establish a connection.)
- `2` (If server authentication fails, show a warning and allow me to connect or refuse the connection.)
- `3` (No authentication requirement is specified.)

### Enable gateway server

Enable the use of a gateway server for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Server name

Hostname of the gateway server used for the remote desktop connection.

**Type:** `String`

**Default:** `Empty`

**Example:** `gateway.borntoberoot.net`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled.

### Bypass for local addresses

Bypass the gateway server for local addresses.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled.

### Logon method

Logon method used for the gateway server.

**Type:** `NETworkManager.Models.RemoteDesktop.GatewayUserSelectedCredsSource`

**Default:** `Allow me to select later`

**Possible values:**

`Ask for password (NTLM)` (`Userpass`)
`Smart card or Windows Hello for Business` (`Smartcard`)
`Allow me to select later` (`Any`)

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled.

### Share gateway credentials with remote computer

Share the gateway credentials with the remote computer.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-2) is enabled.

### Use gateway credentials

Use credentials to authenticate with the gateway server.

**Type:** `Boolean`

**Default:** `Disabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled and [Logon method](#logon-method-1) is set to `Ask for password (NTLM)`.

### Username

Username to authenticate with the gateway server.

**Type:** `String`

**Default:** `Empty`

**Example:** `Administrator`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled, [Logon method](#logon-method-1) is set to `Ask for password (NTLM)` and [Use gateway credentials](#use-gateway-credentials-1) is enabled.

### Domain

Domain to authenticate with the gateway server. This is optional.

**Type:** `String`

**Default:** `Empty`

**Example:** `BORNTOBEROOT`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled, [Logon method](#logon-method-1) is set to `Ask for password (NTLM)` and [Use gateway credentials](#use-gateway-credentials-1) is enabled.

### Password

Password to authenticate with the gateway server.

**Type:** `String`

**Default:** `Empty`

**Example:** `P@ssw0rd`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-1) is enabled, [Logon method](#logon-method-1) is set to `Ask for password (NTLM)` and [Use gateway credentials](#use-gateway-credentials-1) is enabled.

{: .warning }
The profile file should be encrypted when storing sensitive information like passwords. See [FAQ > Profile encryption](NETworkManager/FAQ#profile-encryption) for more details.

### Remote audio playback

Configure the remote audio playback for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.AudioRedirectionMode`

**Default:** `Play on remote computer`

**Possible values:**

- `Play on this computer`
- `Play on remote computer`
- `Do not play`

### Remote audio recording

Configure the remote audio recording for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.AudioCaptureRedirectionMode`

**Default:** `Do not record`

**Possible values:**

- `Record from this computer`
- `Do not record`

### Apply Windows key combinations

Configure the Windows key combinations for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.KeyboardHookMode`

**Default:** `On the remote computer`

**Possible values:**

- `On this computer`
- `On the remote computer`

### Redirect clipboard

Configure the clipboard redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Enabled`

### Redirect devices

Configure the device redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect drives

Configure the drive redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect ports

Configure the port redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect smartcards

Configure the smartcard redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect printers

Configure the printer redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Persitent bitmap caching

Configure the persistent bitmap caching for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Reconnect if the connection is dropped

Automatically reconnect if the connection is dropped.

**Type:** `Boolean`

**Default:** `Disabled`

### Network connection type

Configure the network connection type for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.NetworkConnectionType`

**Default:** `Detect connection quality automatically`

**Possible values:**

- `Detect connection quality automatically`
- `Modem (56 kbps)`
- `Low-speed broadband (256 kbps - 2 Mbps)`
- `Satellite (2 Mbps - 16 Mbps with high latency)`
- `High-speed broadband (2 Mbps - 10 Mbps)`
- `WAN (10 Mbps or higher with high latency)`
- `LAN (10 Mbps or higher)`

## Settings

### Display

Display setting used for the remote desktop connection.

**Type:** `Boolean` (`RadioButtons`)

**Default:** `Use the current view size as screen size`

**Possible values:**

- `Adjust screen automatically` (Remote Desktop will adjust the screen size automatically when the window is resized.)
- `Use the current view size as screen size` (Remote Desktop will use the current view size as screen size, but will not adjust the screen size automatically when the window is resized.)
- `Fixed screen size` (Remote Desktop uses the fixed screen size selected in the drop-down list below.)
  **Possible values:**
  - `640x480`
  - `800x600`
  - `1024x768`
  - `1280x720`
  - `1280x768`
  - `1280x800`
  - `1280x1024`
  - `1360x768`
  - `1440x900`
  - `1440x1050`
  - `1680x1050`
  - `1920x1080`
- `Custom screen size` (Remote Desktop uses the custom screen size that can be entered in the text fields below.)

### Color depth

Color depth used for the remote desktop connection.

**Type:** `Integer`

**Default:** `32`

**Possible values:**

- `15`
- `16`
- `24`
- `32`

### Port

Port used for the remote desktop connection.

**Type:** `Integer`

**Default:** `3389`

### Credential Security Support Provider

Use the Credential Security Support Provider (CredSSP) protocol to authenticate with the remote computer.

**Type:** `Boolean`

**Default:** `Disabled`

### Authentication level

Authentication level used for the remote desktop connection.

**Type:** `Integer`

**Default:** `2` [Min `0`, Max `3`]

**Possible values:**

- `0` (If server authentication fails, connect to the computer without warning.)
- `1` (If server authentication fails, do not establish a connection.)
- `2` (If server authentication fails, show a warning and allow me to connect or refuse the connection.)
- `3` (No authentication requirement is specified.)

### Enable gateway server

Enable the use of a gateway server for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Server name

Hostname of the gateway server used for the remote desktop connection.

**Type:** `String`

**Default:** `Empty`

**Example:** `gateway.borntoberoot.net`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-2) is enabled.

### Bypass for local addresses

Bypass the gateway server for local addresses.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-2) is enabled.

### Logon method

Logon method used for the gateway server.

**Type:** `NETworkManager.Models.RemoteDesktop.GatewayUserSelectedCredsSource`

**Default:** `Allow me to select later`

**Possible values:**

`Ask for password (NTLM)` (`Userpass`)
`Smart card or Windows Hello for Business` (`Smartcard`)
`Allow me to select later` (`Any`)

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-2) is enabled.

### Share gateway credentials with remote computer

Share the gateway credentials with the remote computer.

**Type:** `Boolean`

**Default:** `Enabled`

{: .note }
Only available if [Enable gateway server](#enable-gateway-server-2) is enabled.

### Remote audio playback

Configure the remote audio playback for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.AudioRedirectionMode`

**Default:** `Play on remote computer`

**Possible values:**

- `Play on this computer`
- `Play on remote computer`
- `Do not play`

### Remote audio recording

Configure the remote audio recording for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.AudioCaptureRedirectionMode`

**Default:** `Do not record`

**Possible values:**

- `Record from this computer`
- `Do not record`

### Apply Windows key combinations

Configure the Windows key combinations for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.KeyboardHookMode`

**Default:** `On the remote computer`

**Possible values:**

- `On this computer`
- `On the remote computer`

### Redirect clipboard

Configure the clipboard redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Enabled`

### Redirect devices

Configure the device redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect drives

Configure the drive redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect ports

Configure the port redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect smartcards

Configure the smartcard redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Redirect printers

Configure the printer redirection for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Persitent bitmap caching

Configure the persistent bitmap caching for the remote desktop connection.

**Type:** `Boolean`

**Default:** `Disabled`

### Reconnect if the connection is dropped

Automatically reconnect if the connection is dropped.

**Type:** `Boolean`

**Default:** `Disabled`

### Network connection type

Configure the network connection type for the remote desktop connection.

**Type:** `NETworkManager.Models.RemoteDesktop.NetworkConnectionType`

**Default:** `Detect connection quality automatically`

**Possible values:**

- `Detect connection quality automatically`
- `Modem (56 kbps)`
- `Low-speed broadband (256 kbps - 2 Mbps)`
- `Satellite (2 Mbps - 16 Mbps with high latency)`
- `High-speed broadband (2 Mbps - 10 Mbps)`
- `WAN (10 Mbps or higher with high latency)`
- `LAN (10 Mbps or higher)`
