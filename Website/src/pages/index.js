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

const directDownloads = release.downloads.x64;

function HomepageHeader() {
  return (
    <div className={styles.hero} data-theme="dark">
      <div className={styles.heroInner}>
        <div className={styles.heroHeading}>
          <img
            alt="NETworkManager Logo"
            className={styles.heroLogo}
            src="./img/logo.svg"
            width="200"
            height="200"
          />
          <div className={styles.heroTextHtml}>
            <Heading as="h1" className={styles.heroProjectTitle}>
              <b>NETworkManager</b>
            </Heading>
            <p className={styles.heroProjectTagline}>
              A powerful open-source tool for <b>managing</b> networks and{" "}
              <b>troubleshooting</b> network problems!
            </p>
          </div>
        </div>
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
          <span aria-hidden="true">⬇</span> Direct download (x64):{" "}
          <Link to={directDownloads.setup}>MSI-Setup</Link>
          <span className={styles.directDownloadsSep}>|</span>
          <Link to={directDownloads.portable}>Portable</Link>
        </div>
        <div className={styles.heroScreenshotWrapper}>
          <img
            src={ImageGalleryDashboard}
            alt="NETworkManager Dashboard"
            className={styles.heroScreenshot}
            width={1920}
            height={1200}
            fetchPriority="high"
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
              src="./img/preview-run-command.gif"
              alt="Run command preview"
              width={1280}
              height={800}
              loading="lazy"
              decoding="async"
            />
            <p className={styles.previewCaption}>
              Run command: start any tool from the keyboard with Ctrl+Shift+P.
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
              Explore the documentation →
            </Link>
          </div>
        </div>
      </main>
    </Layout>
  );
}
