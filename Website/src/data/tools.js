// Single source of truth for the built-in tools list, shared by the homepage
// tool grid and the docs introduction. Sorted by sidebar_position.
// `note` (optional) is shown as a tooltip on the chip.
const tools = [
  { name: "Dashboard", to: "/docs/application/dashboard" },
  {
    name: "Network Interface",
    to: "/docs/application/network-interface",
    note: "Information, Bandwidth, Configure",
  },
  { name: "WiFi", to: "/docs/application/wifi", note: "Networks, Channels" },
  { name: "IP Scanner", to: "/docs/application/ip-scanner" },
  { name: "Port Scanner", to: "/docs/application/port-scanner" },
  { name: "Ping Monitor", to: "/docs/application/ping-monitor" },
  { name: "Traceroute", to: "/docs/application/traceroute" },
  { name: "DNS Lookup", to: "/docs/application/dns-lookup" },
  { name: "Remote Desktop", to: "/docs/application/remote-desktop" },
  { name: "PowerShell", to: "/docs/application/powershell" },
  { name: "PuTTY", to: "/docs/application/putty", note: "Requires PuTTY" },
  { name: "TigerVNC", to: "/docs/application/tigervnc", note: "Requires TigerVNC" },
  { name: "Web Console", to: "/docs/application/web-console" },
  { name: "SNMP", to: "/docs/application/snmp", note: "Get, Walk, Set" },
  { name: "SNTP Lookup", to: "/docs/application/sntp-lookup" },
  { name: "Hosts File Editor", to: "/docs/application/hosts-file-editor" },
  { name: "Firewall", to: "/docs/application/firewall" },
  {
    name: "Discovery Protocol",
    to: "/docs/application/discovery-protocol",
    note: "LLDP, CDP",
  },
  { name: "Wake on LAN", to: "/docs/application/wake-on-lan" },
  { name: "Whois", to: "/docs/application/whois" },
  { name: "IP Geolocation", to: "/docs/application/ip-geolocation" },
  {
    name: "Subnet Calculator",
    to: "/docs/application/subnet-calculator",
    note: "Calculator, Subnetting, Supernetting",
  },
  { name: "Bit Calculator", to: "/docs/application/bit-calculator" },
  { name: "Lookup", to: "/docs/application/lookup", note: "OUI, Port" },
  { name: "Connections", to: "/docs/application/connections" },
  { name: "Listeners", to: "/docs/application/listeners" },
  { name: "Neighbor Table", to: "/docs/application/neighbor-table" },
];

export default tools;
