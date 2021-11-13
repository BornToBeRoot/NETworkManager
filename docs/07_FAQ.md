---
layout: default
title: FAQ
nav_order: 6
description: "Frequently Asked Questions"
permalink: /FAQ
---

# Frequently Asked Questions
The documentation and the FAQ are not complete yet! Feel free to ask questions on [GitHub Discussions](https://github.com/BornToBeRoot/NETworkManager/discussions).

## How to contribute or report an issue?
Read the [contributing guidelines](https://github.com/BornToBeRoot/NETworkManager/blob/master/CONTRIBUTING.md){:target="_blank"} and follow the instructions there.

## How to build the project?
The project can be build with Visual Studio or with a PowerShell script. More details and the requirements can be found in the [README](https://github.com/BornToBeRoot/NETworkManager/blob/master/README.md#build){:target="_blank"}.

## Profile and settings priority
If there is a possibility to set a setting in the profile, this setting applies to this host and overwrites the default settings.

## How does the profile encryption work?
Profile files are encrypted on disk using [Rijndael](https://docs.microsoft.com/en-US/dotnet/api/system.security.cryptography.rijndaelmanaged?view=net-5.0){:target="_blank"} (AES) with a key size of 256 bits and a block size of 128 bits in CBC mode. The encryption key is derived from a master password using [Rfc2898DeriveBytes](https://docs.microsoft.com/en-US/dotnet/api/system.security.cryptography.rfc2898derivebytes?view=net-5.0){:target="_blank"} (PBKDF2) with 1,000,000 iterations. At runtime, passwords are stored as [SecureString](https://docs.microsoft.com/en-US/dotnet/api/system.security.securestring?view=net-5.0){:target="_blank"} once the profile file is loaded. For some functions, the password must be converted to a normal string and remains unencrypted in memory until the garbage collector cleans them up.

## How to enable profile file encryption?
Open the settings and go to the profile section. Right click on the profile file you want to encrypt. Select `Encryption...` > `Enable encryption...` and set your master password.

![ProfileFile_EnableEncryption](ProfileFile_EnableEncryption.gif)

## How to change the master password of an encrypted profile file?
Open the settings and go to the profile section. Right click on an encrypted profile file. Select `Encryption...` > `Change Master Password...` and enter the current master password and a new master password.

![ProfileFile_EnableEncryption](ProfileFile_ChangeMasterPassword.gif)

## How to disable profile file encryption?
Open the settings and go to the profile section. Right click on an encrypted profile file. Select `Encryption...` > `Disable encryption...` and enter your master password.

![ProfileFile_DisableEncryption](ProfileFile_DisableEncryption.gif)
