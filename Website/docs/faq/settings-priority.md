# Settings Priority

## How does NETworkManager prioritize settings across different levels (global, group, profile)?

NETworkManager uses a clear hierarchy to apply settings, allowing you to configure profiles with flexibility and precision. Settings are prioritized across three levels, from broadest to most specific:

1. **Global Settings**  
   Default settings applied across all profiles in NETworkManager unless overridden.  
   _Example_: A default SSH key file used for all network devices.

2. **Group Settings**  
   Applied to all profiles within a specific group, overriding global settings. Use these to customize settings for a set of devices.  
   _Example_: A unique SSH key file for a group of servers in a data center.

3. **Profile Settings**  
   Specific to an individual profile, overriding both group and global settings. Use these for fine-tuned configurations on a single device.  
   _Example_: A custom SSH key file for a critical server.

Settings are applied in this order:

**Global Settings (default) → Group Settings (overrides Global) → Profile Settings (overrides Group)**
