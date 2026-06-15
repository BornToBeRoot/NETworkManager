import React from "react";
import Link from "@docusaurus/Link";
import Layout from "@theme/Layout";
import HomepageFeatures from "@site/src/components/HomepageFeatures";
import ToolChips from "@site/src/components/ToolChips";
import tools from "@site/src/data/tools";
import release from "@site/src/data/release.json";

import ImageGalleryDashboard from "@site/docs/img/dashboard.png";

import Heading from "@theme/Heading";
import styles from "./styles.module.css";

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
            <a
              href="https://github.com/BornToBeRoot/NETworkManager/stargazers"
              target="_blank"
              rel="noopener noreferrer"
            >
              <img
                alt="GitHub Stars"
                className={styles.indexCtasGitHubButton}
                src="https://img.shields.io/github/stars/BornToBeRoot/NETworkManager?style=for-the-badge&logo=github&logoColor=white&label=Stars&labelColor=2b3137&color=00d4aa"
                height={28}
                title="GitHub Stars"
              />
            </a>
            <a
              href="https://github.com/BornToBeRoot/NETworkManager/releases"
              target="_blank"
              rel="noopener noreferrer"
            >
              <img
                alt="GitHub Downloads"
                className={styles.indexCtasGitHubButton}
                src="https://img.shields.io/github/downloads/BornToBeRoot/NETworkManager/total?style=for-the-badge&logo=github&logoColor=white&label=Downloads&labelColor=2b3137&color=00d4aa"
                height={28}
                title="GitHub Downloads"
              />
            </a>
          </span>
        </div>
        <div className={styles.directDownloads}>
          <span aria-hidden="true">⬇</span> Direct download:{" "}
          <Link to={release.downloads.setup}>MSI-Setup</Link>
          <span className={styles.directDownloadsSep}>|</span>
          <Link to={release.downloads.portable}>Portable</Link>
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
        <ToolChips />
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
          <div className={styles.previewCtas}>
            <Link className="button button--primary" to="/download">
              ⬇ Download now
            </Link>
            <Link
              className="button button--outline button--primary"
              to="/docs/introduction"
            >
              More screenshots in the docs →
            </Link>
          </div>
        </div>
      </main>
    </Layout>
  );
}
