import React from "react";
import Link from "@docusaurus/Link";
import Layout from "@theme/Layout";
import HomepageFeatures from "@site/src/components/HomepageFeatures";

import ImageGalleryDashboard from "@site/docs/img/dashboard.png";

import Heading from "@theme/Heading";
import styles from "./styles.module.css";

// Sorted by sidebar_position
const tools = [
  { name: "Dashboard", to: "/docs/application/dashboard" },
  { name: "Network Interface", to: "/docs/application/network-interface" },
  { name: "WiFi", to: "/docs/application/wifi" },
  { name: "IP Scanner", to: "/docs/application/ip-scanner" },
  { name: "Port Scanner", to: "/docs/application/port-scanner" },
  { name: "Ping Monitor", to: "/docs/application/ping-monitor" },
  { name: "Traceroute", to: "/docs/application/traceroute" },
  { name: "DNS Lookup", to: "/docs/application/dns-lookup" },
  { name: "Remote Desktop", to: "/docs/application/remote-desktop" },
  { name: "PowerShell", to: "/docs/application/powershell" },
  { name: "PuTTY", to: "/docs/application/putty" },
  { name: "TigerVNC", to: "/docs/application/tigervnc" },
  { name: "Web Console", to: "/docs/application/web-console" },
  { name: "SNMP", to: "/docs/application/snmp" },
  { name: "SNTP Lookup", to: "/docs/application/sntp-lookup" },
  { name: "Hosts File Editor", to: "/docs/application/hosts-file-editor" },
  { name: "Firewall", to: "/docs/application/firewall" },
  { name: "Discovery Protocol", to: "/docs/application/discovery-protocol" },
  { name: "Wake on LAN", to: "/docs/application/wake-on-lan" },
  { name: "Whois", to: "/docs/application/whois" },
  { name: "IP Geolocation", to: "/docs/application/ip-geolocation" },
  { name: "Subnet Calculator", to: "/docs/application/subnet-calculator" },
  { name: "Bit Calculator", to: "/docs/application/bit-calculator" },
  { name: "Lookup", to: "/docs/application/lookup" },
  { name: "Connections", to: "/docs/application/connections" },
  { name: "Listeners", to: "/docs/application/listeners" },
  { name: "Neighbor Table", to: "/docs/application/neighbor-table" },
];

function HomepageHeader() {
  return (
    <div className={styles.hero} data-theme="dark">
      <div className={styles.heroInner}>
        <Heading as="h1">
          <img
            alt="NETworkManager Logo"
            className={styles.heroLogo}
            src="./img/logo.svg"
            width="200"
            height="200"
          />
          <span className={styles.heroTextHtml}>
            <p className={styles.heroProjectTitle}>
              <b>NETworkManager</b>
            </p>
            <p className={styles.heroProjectTagline}>
              A powerful open-source tool for <b>managing</b> networks and{" "}
              <b>troubleshooting</b> network problems!
            </p>
          </span>
        </Heading>
        <div className={styles.indexCtas}>
          <Link className="button button--primary" to="/download">
            Download
          </Link>
          <Link
            className="button button--info"
            to="https://github.com/BornToBeRoot/NETworkManager"
          >
            GitHub
          </Link>
          <span className={styles.indexCtasGitHubButtonWrapper}>
            <img
              alt="GitHub Downloads"
              className={styles.indexCtasGitHubButton}
              src="https://img.shields.io/github/downloads/BornToBeRoot/NETworkManager/total.svg?style=social"
              height={24}
              title="GitHub Downloads"
            />
            <img
              alt="GitHub Stars"
              className={styles.indexCtasGitHubButton}
              src="https://img.shields.io/github/stars/BornToBeRoot/NETworkManager?style=social"
              height={24}
              title="GitHub Stars"
            />
          </span>
        </div>
        <div className={styles.heroScreenshotWrapper}>
          <img
            src={ImageGalleryDashboard}
            alt="NETworkManager Dashboard"
            className={styles.heroScreenshot}
          />
        </div>
      </div>
    </div>
  );
}

function ToolGrid() {
  return (
    <div className={styles.toolGrid}>
      <div className="container text--center">
        <Heading as="h2">{tools.length} Built-in Tools</Heading>
        <p className={styles.toolGridSubtitle}>
          Everything a network engineer needs, in one place.
        </p>
        <div className={styles.toolChips}>
          {tools.map((tool) => (
            <Link key={tool.name} to={tool.to} className={styles.toolChip}>
              {tool.name}
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}

export default function Home() {
  return (
    <Layout
      title="A powerful open-source tool for managing networks and troubleshooting network problems!"
      description="NETworkManager - A powerful open-source tool for managing networks and troubleshooting network problems. 25+ built-in network tools for daily use."
    >
      <HomepageHeader />
      <main>
        <ToolGrid />
        <HomepageFeatures />
        <div className="container text--center margin-top--xl margin-bottom--xl">
          <Heading as="h2">See It in Action</Heading>
          <div className={styles.previewSingle}>
            <img
              src="./img/preview-tabs-drag-drop.gif"
              alt="Tabs and drag & drop preview"
            />
            <p className={styles.previewCaption}>
              Tabs and drag &amp; drop functionality.
            </p>
          </div>
          <Link
            className="button button--outline button--primary"
            to="/docs/introduction"
          >
            More screenshots in the docs →
          </Link>
        </div>
      </main>
    </Layout>
  );
}
