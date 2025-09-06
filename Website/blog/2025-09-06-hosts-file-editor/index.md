---
slug: hosts-file-editor
title: Hosts File Editor
authors: [borntoberoot]
tags: [hosts file, dns, new feature]
---

NETworkManager 2025.8.10.0 introduced a new feature, the `Hosts File Editor`. You can now easily manage and edit your system's hosts file in a user-friendly interface.

As a sysadmin or developer, you often need to modify the hosts file in order to override DNS settings for specific domains, redirect traffic, or test websites locally.

<!-- truncate -->

The hosts file is a plain text file used by the operating system to map hostnames to IP addresses. It is typically located at:

- Windows: `C:\Windows\System32\drivers\etc\hosts`
- Linux: `/etc/hosts`

Editing this file usually requires administrative privileges and can be cumbersome using traditional text editors.
With the new Hosts File Editor in NETworkManager, you can now easily add, edit, enable, disable, or delete entries
in your hosts file without needing to manually open and edit the file.

A daily backup (up to 5 versions) of the hosts file is created automatically before any changes are applied,
allowing you to restore previous versions if needed.

Upgrade now to the latest version of NETworkManager to use this feature: </download>

More information is available in the official documentation: </docs/application/hosts-file-editor>

If you find any issues or have suggestions for improvement, please open an issue on GitHub:
<https://github.com/BornToBeRoot/NETworkManager/issues>
