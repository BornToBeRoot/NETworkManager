import React from "react";
import clsx from "clsx";
import Link from "@docusaurus/Link";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import Layout from "@theme/Layout";
import HomepageFeatures from "@site/src/components/HomepageFeatures";
import ImageGallery from "react-image-gallery";

import ImageGalleryDashboard from "@site/docs/img/dashboard.png";
import ImageGalleryNetworkInterfaceInformation from "@site/docs/img/network-interface--information.png";
import ImageGalleryWiFi from "@site/docs/img/wifi.png";
import ImageGalleryIPScanner from "@site/docs/img/ip-scanner.png";
import ImageGalleryPortScanner from "@site/docs/img/port-scanner.png";
import ImageGalleryPingMonitor from "@site/docs/img/ping-monitor.png";
import ImageGalleryTraceroute from "@site/docs/img/traceroute.png";
import ImageGalleryDNSLookup from "@site/docs/img/dns-lookup.png";
import ImageGalleryRemoteDesktop from "@site/docs/img/remote-desktop.png";
import ImageGalleryPowerShell from "@site/docs/img/powershell.png";
import ImageGalleryPuTTY from "@site/docs/img/putty.png";
import ImageGalleryTigerVNC from "@site/docs/img/tigervnc.png";
import ImageGalleryWebConsole from "@site/docs/img/web-console.png";
import ImageGallerySNTPLookup from "@site/docs/img/sntp-lookup.png";
import ImageGalleryDiscoveryProtocol from "@site/docs/img/discovery-protocol.png";
import ImageGalleryWhois from "@site/docs/img/whois.png";
import ImageGalleryIPGeolocation from "@site/docs/img/ip-geolocation.png";
import ImageGallerySubnetCalculatorCalculator from "@site/docs/img/subnet-calculator--calculator.png";

import Heading from "@theme/Heading";
import styles from "./styles.module.css";

function HomepageHeader() {
  const { siteConfig } = useDocusaurusContext();
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
              A powerful tool for <b>managing</b> networks and{" "}
              <b>troubleshoot</b> network problems!
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
              src="https://img.shields.io/github/downloads/BornToBeroot/NETworkManager/total.svg?style=social"
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
      </div>
    </div>
  );
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();

  const images1 = [
    {
      original: ImageGalleryDashboard,
      thumbnail: ImageGalleryDashboard,
      originalAlt: "Dashboard",
      thumbnailAlt: "Dashboard",
      description: "Dashboard",
    },
    {
      original: ImageGalleryNetworkInterfaceInformation,
      thumbnail: ImageGalleryNetworkInterfaceInformation,
      originalAlt: "Network Interface - Information",
      thumbnailAlt: "Network Interface - Information",
      description: "Network Interface - Information",
    },
    {
      original: ImageGalleryWiFi,
      thumbnail: ImageGalleryWiFi,
      originalAlt: "WiFi",
      thumbnailAlt: "WiFi",
      description: "WiFi",
    },
    {
      original: ImageGalleryIPScanner,
      thumbnail: ImageGalleryIPScanner,
      originalAlt: "IP Scanner",
      thumbnailAlt: "IP Scanner",
      description: "IP Scanner",
    },
    {
      original: ImageGalleryPortScanner,
      thumbnail: ImageGalleryPortScanner,
      originalAlt: "Port Scanner",
      thumbnailAlt: "Port Scanner",
      description: "Port Scanner",
    },
    {
      original: ImageGalleryPingMonitor,
      thumbnail: ImageGalleryPingMonitor,
      originalAlt: "Ping Monitor",
      thumbnailAlt: "Ping Monitor",
      description: "Ping Monitor",
    },
    {
      original: ImageGalleryTraceroute,
      thumbnail: ImageGalleryTraceroute,
      originalAlt: "Traceroute",
      thumbnailAlt: "Traceroute",
      description: "Traceroute",
    },
    {
      original: ImageGalleryDNSLookup,
      thumbnail: ImageGalleryDNSLookup,
      originalAlt: "DNS Lookup",
      thumbnailAlt: "DNS Lookup",
      description: "DNS Lookup",
    },
    {
      original: ImageGalleryRemoteDesktop,
      thumbnail: ImageGalleryRemoteDesktop,
      originalAlt: "Remote Desktop",
      thumbnailAlt: "Remote Desktop",
      description: "Remote Desktop",
    },
    {
      original: ImageGalleryPowerShell,
      thumbnail: ImageGalleryPowerShell,
      originalAlt: "PowerShell",
      thumbnailAlt: "PowerShell",
      description: "PowerShell",
    },
    {
      original: ImageGalleryPuTTY,
      thumbnail: ImageGalleryPuTTY,
      originalAlt: "PuTTY",
      thumbnailAlt: "PuTTY",
      description: "PuTTY",
    },
    {
      original: ImageGalleryTigerVNC,
      thumbnail: ImageGalleryTigerVNC,
      originalAlt: "TigerVNC",
      thumbnailAlt: "TigerVNC",
      description: "TigerVNC",
    },
    {
      original: ImageGalleryWebConsole,
      thumbnail: ImageGalleryWebConsole,
      originalAlt: "Web Console",
      thumbnailAlt: "Web Console",
      description: "Web Console",
    },
    {
      original: ImageGallerySNTPLookup,
      thumbnail: ImageGallerySNTPLookup,
      originalAlt: "SNTP Lookup",
      thumbnailAlt: "SNTP Lookup",
      description: "SNTP Lookup",
    },
    {
      original: ImageGalleryDiscoveryProtocol,
      thumbnail: ImageGalleryDiscoveryProtocol,
      originalAlt: "Discovery Protocol",
      thumbnailAlt: "Discovery Protocol",
      description: "Discovery Protocol",
    },
    {
      original: ImageGalleryWhois,
      thumbnail: ImageGalleryWhois,
      originalAlt: "Whois",
      thumbnailAlt: "Whois",
      description: "Whois",
    },
    {
      original: ImageGalleryIPGeolocation,
      thumbnail: ImageGalleryIPGeolocation,
      originalAlt: "IP Geolocation",
      thumbnailAlt: "IP Geolocation",
      description: "IP Geolocation",
    },
    {
      original: ImageGallerySubnetCalculatorCalculator,
      thumbnail: ImageGallerySubnetCalculatorCalculator,
      originalAlt: "Subnet Calculator - Calculator",
      thumbnailAlt: "Subnet Calculator - Calculator",
      description: "Subnet Calculator - Calculator",
    },
  ];

  return (
    <Layout
      title={`${siteConfig.title}`}
      description={`${siteConfig.title} - ${siteConfig.tagline}`}
    >
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <div className="container text--center margin-top--xl margin-bottom--xl">
          <Heading as="h2">📷 Screenshots</Heading>
          <div className="gallery-container1">
            <ImageGallery
              autoPlay="true"
              items={images1}
              additionalClass={styles.imageScreenshot}
            />
          </div>
          <p>Overview of various features and tools in NETworkManager.</p>
          <br />
          <div className="gallery-container2">
            <img
              src="./img/preview-profiles.gif"
              alt="NETworkManager profiles preview"
              className={styles.imageScreenshot}
            />
          </div>
          <p>Encrypted profile files, group & profile management.</p>
          <br />
          <div className="gallery-container3">
            <img
              src="./img/preview-tabs-drag-drop.gif"
              alt="NETworkManager tabs and drag & drop preview"
              className={styles.imageScreenshot}
            />
          </div>
          <p>Tabs and drag & drop functionality.</p>
          <br />
          <div className="gallery-container4">
            <img
              src="./img/preview-light-theme.png"
              alt="NETworkManager light theme preview"
              className={styles.imageScreenshot}
            />
          </div>
          <p>Customizable light/dark themes and accent colors.</p>
        </div>
      </main>
    </Layout>
  );
}
