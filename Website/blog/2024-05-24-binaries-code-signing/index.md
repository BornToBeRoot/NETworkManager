---
slug: binaries-are-now-signed-with-a-code-signing-certificate
title: Binaries are now signed with a code signing certificate
authors: [borntoberoot]
tags: [code-signing, binaries, installer]
---

Starting with NETworkManager version 2024.5.24.0, the binaries and the installer are now signed with a code signing certificate.

The binaries and installer are built on [AppVeyor](https://ci.appveyor.com/project/BornToBeRoot/networkmanager) directly from the [GitHub repository](https://github.com/BornToBeRoot/NETworkManager/blob/main/appveyor.yml).
Build artifacts are automatically sent to [SignPath.io](https://signpath.io/) via webhook, where they are signed after manual approval by the maintainer.
The signed binaries are then uploaded to the [GitHub releases](https://github.com/BornToBeRoot/NETworkManager/releases) page.

Special thanks goes to [SignPath.io](https://signpath.io/) for providing free code signing and the [SignPath Foundation](https://signpath.org/) for the free code signing certificate.
