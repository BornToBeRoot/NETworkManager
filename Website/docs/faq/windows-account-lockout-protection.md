---
description: "How the IP Scanner protects Windows / Active Directory accounts from being locked out when it reads the Windows host information with credentials: one host checks them first, failures are counted per account, rejected credentials are not used again."
keywords: [NETworkManager, IP Scanner, account lockout, Active Directory, Windows credentials, NetApi, SMB, NTLM, Kerberos, badPwdCount, lockout threshold]
---

# Windows account lockout protection

## Why does the IP Scanner need a lockout protection?

With [Read Windows host information](../application/ip-scanner#read-windows-host-information), the IP Scanner logs on to every found Windows host (NetApi over SMB, TCP port `445`) with the [Windows credentials](../application/ip-scanner#windows-credentials) of a profile, a group or the tab. With a wrong password, every host in a domain makes a domain controller count a failed logon for the account (`badPwdCount`). A scan of a `/24` with many Windows hosts runs dozens of logons within seconds - a typical lockout threshold of 5 or 10 failed logons would be reached at once and the account would be locked in the whole domain.

The lockout protection makes sure that a wrong password is tried on as few hosts as possible: the credentials are checked on **one host at a time** until a host proves that they are correct, and they are **rejected** (not used again) as soon as a domain controller may have counted too many failed logons.

The user running NETworkManager (no credentials set) is not checked: Windows uses the logon of the current session, there is no stored password that could be wrong.

## How does it work?

1. **Without credentials first:** the SMB / NTLM negotiation (no logon, no failed logon) tells the computer name and the domain or workgroup of the host. A host in a domain is _gated_ (see [below](#which-hosts-are-not-gated)).
2. **One host checks the credentials**, the other gated hosts wait. A host that waits longer than twice the [timeout](../application/ip-scanner#timeout-ms-2) is not tried (`Not tried: the Windows credentials were still being checked on another host ...`). The host that checks keeps its turn until Windows returns, also after its timeout - two hosts never check them at the same time, also not in different tabs (one turn per account).
3. **The result of that host decides** (see the table below): the credentials are _accepted_ (all hosts use them in parallel), _rejected_ (no other host uses them) or the next host checks them.
4. **While the hosts use accepted credentials**, a failed logon on a host in their domain (e.g. the password was changed or the account locked during the scan) rejects them for the hosts that didn't start yet (at most 16 hosts run at the same time). A failure on a host of another domain doesn't (expected also with a correct password, e.g. a domain that is not trusted).
5. **Every scan checks them again** on one host first (e.g. the password was changed or expired since the last scan, also with **Automatically scan every** and **Rescan**). A rejection is kept for the tab.

| Result on the host that checks them | Host in the domain of the credentials | Host in another domain | Credentials without domain (e.g. `admin`) |
| --- | --- | --- | --- |
| Success, `Access is denied.` (logged on without rights), logon type not granted | **Accepted** (only by a Windows host), the counted failures of the account are reset | This host only, the next host checks them | This host only, the next host checks them |
| Failed logon (e.g. wrong password, disabled / locked / expired account) or an unknown error | **Rejected** | Counted, **rejected** after [Max. logon failures](../application/ip-scanner#max-logon-failures) | **Rejected** |
| Logon not allowed (logon hours, workstation, account restriction) | Not counted, the next host checks them | Not counted, the next host checks them | Not counted, the next host checks them |
| No decision (timeout, host / RPC / its domain controller not reachable, NTLM blocked, the requests were not sent) | The next host checks them | The next host checks them | The next host checks them |

### When is a host in the domain of the credentials?

The domain of the credentials is the domain of `DOMAIN\user`, the **Domain** field or the suffix of a UPN (`user@corp.example.com`). A host is in this domain if its NTLM negotiation names it as its NetBIOS domain (`CORP`), its DNS domain (`corp.example.com`) or its forest (`example.com`, e.g. for `admin@example.com`). Only such a host is checked by a domain controller that knows the account - its answer proves the password, or proves that a failed logon was counted.

Only a **Windows** host (it sends its build in the NTLM negotiation) accepts the credentials for all hosts: a Samba server or NAS in the domain could map a wrong password to guest (`map to guest = bad password`) and answer it with a success. A logon failure on such a host still rejects them.

### Why doesn't a success on another host accept the credentials?

A host outside the domain of the credentials doesn't prove the password: a NAS or Samba server that maps a failed logon to guest (`map to guest = bad user`) or a computer with a local account of the same name (and the same password) also answers a wrong password with a success. Only this host uses the credentials, the next host still checks them. For credentials without domain or with a UPN suffix that is not the domain of the hosts, the hosts stay one at a time - slower, but a wrong password can't reach all hosts.

### Why does an unknown error count as a failed logon?

A wrong password is not always reported as `The user name or password is incorrect.` (e.g. errors of Kerberos, an expired account). Only the errors that show that no domain controller checked the password let the next host check them. Every other error counts - an error that is not known can't let all hosts try a wrong password.

## How are the failed logons counted?

- **Per account for all tabs** (e.g. `CORP\admin`; `admin@corp.example.com` and `corp.example.com\admin` are the same account, `CORP\admin` is counted separately), until NETworkManager is closed - like the domain controller counts them. Another tab with the same account (**Scan as...**, the same profile) or credentials set again don't start again from zero.
- **Max. logon failures** (setting, default `2`, `1` - `5`) only applies to hosts of other domains (e.g. a trusted domain, whose domain controller passes the logon on to the domain of the account). A failure in the domain of the credentials or without domain rejects them right away.
- **Accepted in the domain resets the counter**, like a successful logon resets `badPwdCount` at the domain controller.
- **Credentials entered again or with another password are checked once** (e.g. a corrected password) - also if the account already reached the limit; a failure then rejects them right away.

:::warning

A domain controller may count one wrong password twice (Kerberos first, then NTLM). Keep **Max. logon failures** below half of the account lockout threshold of your domain.

:::

## What happens when the credentials are rejected?

- The other hosts don't use them: `Not tried: the Windows credentials were rejected on another host ...` in the row details (export: `WindowsError` `-1`).
- A message is shown in the tab (also if a check that exceeded the timeout rejects them after the scan).
- The next scans of the tab don't use them for the domain hosts (also **Automatically scan every** and **Rescan**).
- **Other tabs with the same account and password don't use them either**, also new ones (e.g. another tab of the same profile or group) - until NETworkManager is closed. The password itself is not kept: only a keyed hash in memory (HMAC-SHA256 with a random key per start of NETworkManager) to recognize it.
- To use them again, enter them with **Options** > **Set Windows credentials...** or **Scan as...** (also unchanged, e.g. after the account was unlocked), or correct the password in the profile / group and open a new tab.

## Which hosts are not gated?

Hosts whose logon can't count at a domain controller use the credentials right away, independent of the other hosts and of a rejection:

- **Computers in a workgroup** (the NTLM negotiation sends the computer name as domain): they check the credentials with their own local accounts.
- **Local accounts** (`.\admin`, or the computer name as domain): `.` is replaced by the computer name of each host (`PC01\admin`), so only the host checks it - never a domain controller.

These hosts are always asked by their IP address (NTLM): with their DNS name, Windows could use Kerberos and first ask the domain controller of the credentials for a ticket - a wrong password would count there for each of them.

A host that rejected the credentials itself is not tried again in the next scans of the tab (one failed logon per host and tab, not per scan): `Not tried: ... or by this host in an earlier scan`.

A host that doesn't send its computer and domain name (NTLM disabled, or incomplete values) is gated like a domain host: it is not known if a domain controller checks its logon.

## What is logged?

The checks are written to the [log file](./file-locations) (`NETworkManager.log`), never the password: the account as used for the logon (e.g. `CORP\admin`), the host as IP address with the computer name and domain / workgroup it sends (e.g. `10.0.0.5 (CLIENT01, domain CORP)`) and the Windows error.

| Level   | Messages |
| ------- | -------- |
| `WARN`  | Every failed logon (with the failures counted for the account), the rejection with its reason, the failed logon of a host that is not gated |
| `INFO`  | Start of a scan with credentials (max. logon failures, failures counted, or rejected in an earlier scan), accepted, logged on outside the domain, logon not allowed, the number of hosts of the scan that didn't use them |
| `DEBUG` | The host that checks the credentials, errors without a decision, a host skipped after its own failure |

## What is not covered?

- **Other applications and computers** that use the same account (e.g. a mapped drive with an old password) also count failed logons - the IP Scanner only knows its own.
- **The counter of NETworkManager doesn't expire**: the domain controller resets `badPwdCount` after the lockout observation window (e.g. 15 minutes), NETworkManager only when the credentials are accepted in the domain or NETworkManager is restarted.
- **A host you don't control** could pretend to be in the domain of the credentials and accept a wrong password - only scan networks you trust with privileged accounts (see the warning in [Read Windows host information](../application/ip-scanner#read-windows-host-information)).

## Recommendations

- Use an account with only the rights you need, e.g. a dedicated account for the inventory.
- Prefer `DOMAIN\user` or the UPN of the domain (`user@corp.example.com`): only then can a host accept the credentials for all hosts.
- Test new credentials on a small range first (e.g. one server), the result is shown in the row details.
- Keep **Max. logon failures** low, at most half of the lockout threshold of your domain.
