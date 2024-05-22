---
slug: binaries-are-now-signed-with-a-code-signing-certificate
title: Binaries are now signed with a code signing certificate
authors: [borntoberoot]
tags: [code-signing, networkmanager, binaries, certificate, installer]
---

Starting with version 2024.x.x of NETworkManager, the binaries and the installer are now signed with a code signing certificate.

Special thanks goes to [SignPath.io](https://signpath.io/) for providing free code signing and the [SignPath Foundation](https://signpath.org/) for the free code signing certificates.

From now on, the binaries and the installer are built on [AppVeyor](https://ci.appveyor.com/project/BornToBeRoot/networkmanager) from the [GitHub repository](https://github.com/BornToBeRoot/NETworkManager/blob/main/appveyor.yml).
The build artifacts are sent automatically to [SignPath.io](https://signpath.io/) via webhook and signed after manual approval by the maintainer. The signed binaries are then uploaded to the [GitHub release](https://github.com/BornToBeRoot/NETworkManager/releases).
