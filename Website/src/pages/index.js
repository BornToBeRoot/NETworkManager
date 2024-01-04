import clsx from "clsx";
import Link from "@docusaurus/Link";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import Layout from "@theme/Layout";
import HomepageFeatures from "@site/src/components/HomepageFeatures";

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

function HomepagePreview() {
  return (
    <div className="container text--center margin-top--xl margin-bottom--xl">
      <Heading as="h2">Check out the preview</Heading>
      <img
        src="./img/preview.gif"
        alt="NETworkManager Preview"
        width="100%"
        height="100%"
      />
    </div>
  );
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout
      title={`${siteConfig.title}`}
      description={`${siteConfig.title} - ${siteConfig.tagline}`}
    >
      <HomepageHeader />
      <main>
        <HomepageFeatures />
        <HomepagePreview />
      </main>
    </Layout>
  );
}
