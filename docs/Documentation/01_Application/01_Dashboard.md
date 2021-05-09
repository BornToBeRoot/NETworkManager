---
layout: default
title: Dashboard
parent: Application
grand_parent: Documentation
nav_order: 1
description: ""
permalink: /Documentation/Application/Dashboard
---

# Dashboard
The dashboard checks automatically and whenever the status of the local network adapters changes (network cable plugged in, WLAN connected, etc.) whether there is a connection to the router and the Internet.

![Dashboard](01_Dashboard.png)

## Settings

### Public IPv4 address
A public IPv4 address that is reachable via ICMP.

**Default** 
```
1.1.1.1
```

### Public IPv6 address
A public IPv6 address that is reachable via ICMP.

**Default** 
```
2606:4700:4700::1111
```

### Check public IP address
Enables or disables the resolution of the public IP address via [api.ipify.org](https://www.ipify.org/){:target="_blank"} and [api6.ipify.org](https://www.ipify.org/){:target="_blank"}.

**Default** 
```
Enabled
```

### Use custom IPv4 address API
Override the default IPv4 address API to resolve the public IP address. The API should return only a plain text IPv4 address like `xx.xx.xx.xx`.

**Default** 
```
Disabled
``` 

**Examples Values:** 
- [`api.ipify.org`](https://api.ipify.org/){:target="_blank"}
- [`ip4.seeip.org`](https://ip4.seeip.org/){:target="_blank"}
- [`api.my-ip.io/ip`](https://api.my-ip.io/ip){:target="_blank"}

### Use custom IPv6 address API
Override the default IPv6 address API to resolve the public IP address. The API should return only a plain text IPv6 address like `xxxx:xx:xxx::xx`.

**Default** 
```
Disabled
```

**Examples Values** 
- [`api6.ipify.org`](https://api6.ipify.org/){:target="_blank"}