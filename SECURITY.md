# Security Policy

> [!NOTE]
> NETworkManager is a free, open-source project maintained in my spare time. There is **no warranty**, **no paid support**, and **no guaranteed response time**. I do my best to address security issues, but please understand that fixes depend on my available free time.

## Supported Versions

Only the latest release is considered for security fixes. Please ensure you are using the most recent version before reporting a vulnerability.

| Version | Supported          |
| ------- | ------------------ |
| Latest  | :white_check_mark: |
| Older   | :x:                |

## Reporting a Vulnerability

> [!IMPORTANT]
> Please **do not** open a public GitHub issue for security vulnerabilities.

If you discover a security vulnerability in NETworkManager, please report it responsibly through [GitHub Security Advisory](https://github.com/BornToBeRoot/NETworkManager/security/advisories/new).

### What to Include

To help us triage and resolve the issue quickly, please provide:

- A clear description of the vulnerability
- Steps to reproduce the issue
- Affected version(s)
- Potential impact (e.g., data exposure, remote code execution)
- Any suggested fixes or mitigations (optional)

### What to Expect

This project is maintained on a **best-effort basis** in my free time. That said, I take security seriously and will do my best to:

- Acknowledge your report as soon as I can.
- Work on a fix or mitigation when time permits.
- Credit you in the release notes (unless you prefer to remain anonymous).

Please be patient — there are no guaranteed timelines.

### Scope

The following are in scope for security reports:

- NETworkManager application code (all modules in `Source/`)
- Profile encryption and credential handling
- Network communication and protocol implementations
- Installer and update mechanisms
- Dependencies shipped with the application

The following are **out of scope**:

- Third-party tools launched by NETworkManager (e.g., PuTTY, TigerVNC)
- The documentation website ([borntoberoot.net/NETworkManager](https://borntoberoot.net/NETworkManager))
- Social engineering attacks

## Code Signing

Official releases are signed via [SignPath.io](https://signpath.io/) through the [SignPath Foundation](https://signpath.org/). Always verify that you are using a signed binary from the official [GitHub Releases](https://github.com/BornToBeRoot/NETworkManager/releases) page or a trusted package manager (`winget`, `choco`).
