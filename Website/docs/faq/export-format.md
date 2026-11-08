---
description: "Format of the CSV, XML and JSON exports of NETworkManager: columns, empty values, numbers, units, dates, MAC addresses and lists."
keywords: [NETworkManager, export, CSV, XML, JSON, export format, columns, script]
---

# Export format

## How are the results exported?

Most tools can export their results with **Export...** (selected or all results) to a CSV, XML or JSON file. All tools use the same format, so a script can read the exports of every tool the same way. The values are never translated: they are the same in every language.

| File | Structure                                                                                                                     |
| ---- | ----------------------------------------------------------------------------------------------------------------------------- |
| CSV  | First line with the column names, one line per result. Values are separated by `,`                                            |
| XML  | `<Tool><Types><Type>...</Type></Types></Tool>` (e.g. `<PingMonitor><PingInfos><PingInfo>`), one element per column            |
| JSON | Array with one object per result, one key per column                                                                          |

The CSV columns, the XML elements and the JSON keys have the same names and order. The columns are in a logical order: like they are shown in the tool, related values next to each other (e.g. the ping or the web page values of the **IP Scanner**). A new column is added where it belongs, so a script should read the columns by name (CSV header, XML element, JSON key) instead of by position.

## Which results are exported?

**Export...** exports what is shown in the result: **All** exports the results shown (e.g. filtered by the search) in the order shown (sorting, grouping), **Selected** the selected results in the order shown (not in the order they were selected).

## How are the values formatted?

| Value                   | Format                                                                                                                                                                       |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Empty value             | Empty in CSV, empty element in XML, `null` in JSON (e.g. the ping time without a reply)                                                                                     |
| Number                  | Decimal point in every language (e.g. `10.5`), without a unit. JSON: number                                                                                                |
| Time / duration         | Number in a fixed unit: ping time in milliseconds (e.g. `4`), SNTP **Offset** in seconds and **RoundTripDelay** in milliseconds, WiFi **BeaconInterval** in milliseconds and **Uptime** in seconds, **Speed** of a network interface in bit/s |
| Yes / No                | `True` / `False` (CSV), `true` / `false` (XML, JSON)                                                                                                                         |
| Date and time           | `yyyy-MM-dd HH:mm:ss.fff` (e.g. `2026-11-08 10:15:30.123`) in the time shown in the tool: local time, **SNTP Lookup** in UTC                                                 |
| Status, type, state     | English name (e.g. `Success`, `TimedOut`, `Open`, `Tcp`), several values separated by `, ` (e.g. `ICMP, TCP`)                                                               |
| MAC address             | `AA:BB:CC:DD:EE:FF`                                                                                                                                                          |
| List (e.g. IP addresses) | One value, separated by `; ` (e.g. `10.0.0.53; 10.0.0.54`)                                                                                                                 |

In the CSV file, text that may contain a comma (e.g. hostname, description, comment) is always enclosed in quotes (`"`), any other value only if it contains a comma, a quote or a line break. A quote in a value is doubled (`""`), as defined in [RFC 4180](https://www.rfc-editor.org/rfc/rfc4180), e.g. for `Import-Csv` in PowerShell.

The ports of a host in the **IP Scanner** export are one value in the CSV file (`Port/Protocol/Service/Description/State`, ports separated by `;`), one `PortInfo` element per port in the XML file and an array of objects in the JSON file.
