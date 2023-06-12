---
layout: default
title: WebConsole
parent: Application
grand_parent: Documentation
nav_order: 14
description: "Documentation of the WebConsole"
permalink: /Documentation/Application/WebConsole
---

# Web Console

The **Web Console** is designed to browse the website of a host (e.g. server, switch, router, dashboard, etc.) to display informations or to configure it. The connection can be established via a profile (double-click, Enter key or right-click `Connect`) or directly via the [connection](#connect) dialog.  

Right-click on the tab will open the context menu with the following options:

{: .note}
WebView2 must be installed on the local computer in order to use this feature. You can download the latest version of WebView2 from the [official website](https://developer.microsoft.com/de-de/microsoft-edge/webview2/){:target="\_blank"}.

- **Reload** - Reload the website.

![WebConsole](14_WebConsole.png)

## Connect

URL of the website to display in the web console.

**Type:** `String`

**Example:** `https://pihole.borntoberoot.net/admin/`

## Profile

### URL

URL of the website to display in the web console.

**Type:** `String`

**Default:** `Empty`

**Example:** `https://pihole.borntoberoot.net/admin/`

## Settings

### Show address bar

Show or hide the navigation / address bar.

**Type:** `Boolean`

**Default:** `Enabled`
