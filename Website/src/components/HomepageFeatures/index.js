import clsx from "clsx";
import Heading from "@theme/Heading";
import styles from "./styles.module.css";

const FeatureList = [
  {
    title: "Unified Experience",
    Svg: require("@site/static/img/undraw_connected_world_wuay.svg").default,
    description: (
      <>
        All your essential network tools in one sleek interface — no more
        juggling apps. Perform in-depth network diagnostics with WiFi Analyzer,
        IP Scanner, Port Scanner, Ping Monitor, Traceroute, DNS Lookup,
        LLDP/CDP Capture, and many more.
      </>
    ),
  },
  {
    title: "Remote System Management",
    Svg: require("@site/static/img/undraw_server_cluster_jwwq.svg").default,
    description: (
      <>
        Connect to remote systems seamlessly via RDP, PuTTY (SSH, Telnet,
        Serial), PowerShell (WinRM), TigerVNC (VNC), or AWS Session Manager.
        Manage your network and server infrastructure efficiently.
      </>
    ),
  },
  {
    title: "Secure Profiles",
    Svg: require("@site/static/img/undraw_personal_settings_re_i6w4.svg")
      .default,
    description: (
      <>
        Organize hosts and networks in encrypted profiles for seamless access
        across all features while protecting sensitive data and maintaining
        easy access to your infrastructure.
      </>
    ),
  },
  {
    title: "Enterprise-Ready",
    Svg: require("@site/static/img/undraw_server_down_s-4-lk.svg").default,
    description: (
      <>
        Professional deployment ready with MSI installer, signed binaries, and
        package manager support (Chocolatey, Evergreen, WinGet). Built for
        enterprise environments.
      </>
    ),
  },
  {
    title: "Open Source & Free",
    Svg: require("@site/static/img/undraw_version_control_re_mg66.svg").default,
    description: (
      <>
        No ads, no subscriptions, fully community-driven. Review the code,
        build it yourself, or contribute to make it even better.
      </>
    ),
  },
];

function Feature({ Svg, title, description }) {
  return (
    <div className={clsx("col col--4")}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  const firstRow = FeatureList.slice(0, 3);
  const secondRow = FeatureList.slice(3);

  // Return 2 rows with 3 columns each
  return (
    <section className={styles.features}>
      <div className="container text--center">
        <div className="row">
          {firstRow.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
        <div className={`row ${styles.featuresRowCenter}`}>
          {secondRow.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
