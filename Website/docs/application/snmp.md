---
sidebar_position: 14
---

# SNMP

With **SNMP** you can manage and monitor network devices using the Simple Network Management Protocol (SNMP). Version `1`, `2c` and `3` with `get`, `walk` and `set` are supported.

Example inputs:

| Host        | Mode   | Version | OID                  | Community | Security       | Username | Auth  | Auth     | Priv  |          |
| ----------- | ------ | ------- | -------------------- | --------- | -------------- | -------- | ----- | -------- | ----- | -------- |
| `server-01` | `Get`  | `V2C`   | `.1.3.6.1; .1.3.6.2` | `public`  | `-/-`          | `-/-`    | `-/-` | `-/-`    | `-/-` | `-/-`    |
| `10.0.0.10` | `Walk` | `V3`    | `.1.3.6.1.2.1.1`     | `-/-`     | `NoAuthNoPriv` | `Admin`  | `-/-` | `-/-`    | `-/-` | `-/-`    |
| `10.0.0.10` | `Walk` | `V3`    | `.1.3.6.1.2.1.1`     | `-/-`     | `AuthNoPriv`   | `Admin`  | `SHA1 | S3cr3t!` | `-/-` | `-/-`    |
| `10.0.0.10` | `Walk` | `V3`    | `.1.3.6.1.2.1.1`     | `-/-`     | `AuthPriv`     | `Admin`  | `SHA1 | S3cr3t!` | `AES  | S3cr3t%` |

:::note

Multiple OIDs (`.1.3.6.1.2.1.1; .1.3.6.1.2.1.2`) can be specified when using the mode `get`.

:::

![SNMP](../img/snmp.png)

:::note

Right-click on the result to copy or export the information.

:::

## Profile

### Inherit host from general

Inherit the host from the general settings.

**Type:** `Boolean`

**Default:** `Enabled`

:::note

If this option is enabled, the [Host](#host) is overwritten by the host from the general settings and the [Host](#host) is disabled.

:::

### Host

Host to connect to via SNMP.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `server-01.borntoberoot.net`
- `10.0.0.10`

### OID

OID for the `get`, `walk` or `set` SNMP request.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `.1.3.6.1.2.1.1`

:::note

Multiple OIDs (`.1.3.6.1.2.1.1; .1.3.6.1.2.1.2`) can be specified when using the [mode `get`](#mode).

:::

### Mode

SNMP mode for the request.

**Type:** `NETworkManager.Models.Network.SNMPMode`

**Default:** `Get`

**Possible values:**

- `Get`
- `Walk`
- `Set`
- `Trap` (not implemented)
- `Inform` (not implemented)

### Version

SNMP version for the request.

**Type:** `NETworkManager.Models.Network.SNMPVersion`

**Default:** `V2C`

**Possible values:**

- `V1`
- `V2C`
- `V3`

### Community

Community for the SNMP `v1` or `v2c` request.

**Type:** `String`

**Default:** `Empty`

:::note

Only available when using [version `1` or `2c`](#version).

:::

:::warning

Passwords are stored in plain text in the profile file unless [Profile file encryption](../faq/profile-file-encryption) is enabled.

:::

### Security

Security for the SNMP `v3` request.

**Type:** `NETworkManager.Models.Network.SNMPV3Security`

**Default:** `AuthPriv`

**Possible values:**

- `NoAuthNoPriv`
- `AuthNoPriv`
- `AuthPriv`

:::note

Only available when using [version `3`](#version).

:::

### Username

Username for the SNMP `v3` request.

**Type:** `String`

**Default:** `Empty`

:::note

Only available when using [version `3`](#version).

:::

### Auth

Authentication for the SNMP `v3` request.

**Type:** `NETworkManager.Models.Network.SNMPV3AuthenticationProvider`

**Default:** `SHA1`

**Possible values:**

- `MD5`
- `SHA1`
- `SHA256`
- `SHA384`
- `SHA512`

:::note

Only available when using [version `3`](#version) and [security `AuthNoPriv` or `AuthPriv`](#security).

:::

:::warning

Passwords are stored in plain text in the profile file unless [Profile file encryption](../faq/profile-file-encryption) is enabled.

:::

### Priv

Privacy for the SNMP `v3` request.

**Type:** `NETworkManager.Models.Network.SNMPV3PrivacyProvider`

**Default:** `AES`

**Possible values:**

- `DES`
- `AES` (128)
- `AES192`
- `AES256`

:::note

Only available when using [version `3`](#version) and [security `AuthPriv`](#security).

:::

:::warning

Passwords are stored in plain text in the profile file unless [Profile file encryption](../faq/profile-file-encryption) is enabled.

:::

## Group

### OID

OID for the `get`, `walk` or `set` SNMP request.

**Type:** `String`

**Default:** `Empty`

**Example:**

- `.1.3.6.1.2.1.1`

:::note

Multiple OIDs (`.1.3.6.1.2.1.1; .1.3.6.1.2.1.2`) can be specified when using the [mode `get`](#mode-1).

:::

### Mode

SNMP mode for the request.

**Type:** `NETworkManager.Models.Network.SNMPMode`

**Default:** `Get`

**Possible values:**

- `Get`
- `Walk`
- `Set`
- `Trap` (not implemented)
- `Inform` (not implemented)

### Version

SNMP version for the request.

**Type:** `NETworkManager.Models.Network.SNMPVersion`

**Default:** `V2C`

**Possible values:**

- `V1`
- `V2C`
- `V3`

### Community

Community for the SNMP `v1` or `v2c` request.

**Type:** `String`

**Default:** `Empty`

:::note

Only available when using [version `1` or `2c`](#version-1).

:::

:::warning

Passwords are stored in plain text in the profile file unless [Profile file encryption](../faq/profile-file-encryption) is enabled.

:::

### Security

Security for the SNMP `v3` request.

**Type:** `NETworkManager.Models.Network.SNMPV3Security`

**Default:** `AuthPriv`

**Possible values:**

- `NoAuthNoPriv`
- `AuthNoPriv`
- `AuthPriv`

:::note

Only available when using [version `3`](#version-1).

:::

### Username

Username for the SNMP `v3` request.

**Type:** `String`

**Default:** `Empty`

:::note

Only available when using [version `3`](#version-1).

:::

### Auth

Authentication for the SNMP `v3` request.

**Type:** `NETworkManager.Models.Network.SNMPV3AuthenticationProvider`

**Default:** `SHA1`

**Possible values:**

- `MD5`
- `SHA1`
- `SHA256`
- `SHA384`
- `SHA512`

:::note

Only available when using [version `3`](#version-1) and [security `AuthNoPriv` or `AuthPriv`](#security-1).

:::

:::warning

Passwords are stored in plain text in the profile file unless [Profile file encryption](../faq/profile-file-encryption) is enabled.

:::

### Priv

Privacy for the SNMP `v3` request.

**Type:** `NETworkManager.Models.Network.SNMPV3PrivacyProvider`

**Default:** `AES`

**Possible values:**

- `DES`
- `AES` (128)
- `AES192`
- `AES256`

:::note

Only available when using [version `3`](#version-1) and [security `AuthPriv`](#security-1).

:::

:::warning

Passwords are stored in plain text in the profile file unless [Profile file encryption](../faq/profile-file-encryption) is enabled.

:::

## Settings

### OID profiles

List of common OIDs for the `get`, `walk` or `set` SNMP request.

**Type:** `NETworkManager.Models.Network.SNMPOIDProfileInfo`

**Default:**

| Description                                               | OID                                                                                                 | Mode |
| --------------------------------------------------------- | --------------------------------------------------------------------------------------------------- | ---- |
| HOST-RESOURCES-MIB                                        | .1.3.6.1.2.1.25                                                                                     | Walk |
| IF-MIB                                                    | .1.3.6.1.2.1.2                                                                                      | Walk |
| IP-MIB                                                    | .1.3.6.1.2.1.4                                                                                      | Walk |
| Linux - Interface names                                   | .1.3.6.1.2.1.2.2.1.2                                                                                | Walk |
| Linux - Load (1, 5, 15 min)                               | .1.3.6.1.4.1.2021.10.1.3.1; .1.3.6.1.4.1.2021.10.1.3.2; .1.3.6.1.4.1.2021.10.1.3.3                  | Get  |
| Linux - Memory (Swap size, total RAM, RAM used, RAM free) | .1.3.6.1.4.1.2021.4.3.0; .1.3.6.1.4.1.2021.4.5.0; .1.3.6.1.4.1.2021.4.6.0; .1.3.6.1.4.1.2021.4.11.0 | Get  |
| Linux - SNMP uptime                                       | .1.3.6.1.2.1.1.3.0                                                                                  | Get  |
| Linux - System uptime                                     | .1.3.6.1.2.1.25.1.1.0                                                                               | Get  |
| SNMPv2-MIB (system)                                       | .1.3.6.1.2.1.1                                                                                      | Walk |
| TCP-MIB                                                   | .1.3.6.1.2.1.6                                                                                      | Walk |
| UDP-MIB                                                   | .1.3.6.1.2.1.7                                                                                      | Walk |
| UCD-SNMP-MIB                                              | .1.3.6.1.4.1.2021                                                                                   | Walk |

:::note

Right-click on a selected OID profile to `edit` or `delete` it.

You can also use the Hotkeys `F2` (`edit`) or `Del` (`delete`) on a selected OID profile.

:::

### Walk mode

Walk mode for the SNMP request.

**Type:** `Lextm.SharpSnmpLib.Messaging.WalkMode`

**Default:** `WithinSubtree`

**Possible values:**

- `Default`
- `WithinSubtree`

### Timeout (ms)

Timeout in milliseconds after which the SNMP request is canceled.

**Type:** `Integer` [Min `100`, Max `900000`]

**Default:** `60000`

### Port

UDP port to use for SNMP requests.

**Type:** `Integer` [Min `1`, Max `65535`]

**Default:** `161`
