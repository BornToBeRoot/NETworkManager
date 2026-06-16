# SNMP Test Environment

A minimal, self-contained SNMP test environment running on Linux (Debian/Ubuntu) to test NETworkManager's SNMP features.

> **Warning:** This configuration is intentionally insecure. Use in isolated test environments only, never in production.

## Requirements

- Debian / Ubuntu (tested on Ubuntu 22.04+)
- Root / `sudo` access

## Setup

Run the setup script to install and configure the SNMP daemon:

```bash
sudo bash setup.sh
```

The script is idempotent — re-running it overwrites the configuration and resets the SNMPv3 credentials.

To customise community strings or credentials, edit the variables at the top of `setup.sh` before running.

## Default credentials

| Protocol            | Setting       | Value            |
| ------------------- | ------------- | ---------------- |
| SNMPv2c             | Community     | `public`         |
| SNMPv3 noAuthNoPriv | Username      | `noAuthUser`     |
| SNMPv3 authNoPriv   | Username      | `authNoPrivUser` |
| SNMPv3 authNoPriv   | Auth protocol | SHA-512          |
| SNMPv3 authNoPriv   | Auth password | `auth123456789`  |
| SNMPv3 authPriv     | Username      | `authPrivUser`   |
| SNMPv3 authPriv     | Auth protocol | SHA-512          |
| SNMPv3 authPriv     | Auth password | `auth123456789`  |
| SNMPv3 authPriv     | Priv protocol | AES              |
| SNMPv3 authPriv     | Priv password | `priv987654321`  |

## Accessible OIDs

The `systemonly` view restricts access to:

| OID prefix          | Description                                    |
| ------------------- | ---------------------------------------------- |
| `.1.3.6.1.2.1.1`    | System group (sysDescr, sysName, sysUpTime, …) |
| `.1.3.6.1.2.1.25.1` | Host resources — hrSystem group                |

## Ports

The daemon listens on UDP port **161** for both IPv4 (`udp:161`) and IPv6 (`udp6:161`).

## Verifying the setup

```bash
# SNMPv2c
snmpwalk -v2c -c public localhost .1.3.6.1.2.1.1

# SNMPv3 – noAuthNoPriv
snmpwalk -v3 -l noAuthNoPriv -u noAuthUser localhost .1.3.6.1.2.1.1

# SNMPv3 – authNoPriv
snmpwalk -v3 -l authNoPriv -u authNoPrivUser \
  -a SHA-512 -A auth123456789 \
  localhost .1.3.6.1.2.1.1

# SNMPv3 – authPriv
snmpwalk -v3 -l authPriv -u authPrivUser \
  -a SHA-512 -A auth123456789 \
  -x AES    -X priv987654321 \
  localhost .1.3.6.1.2.1.1
```

## Additional configuration

Drop extra `.conf` files into `/etc/snmp/snmpd.conf.d/` — they are included automatically by the daemon.
