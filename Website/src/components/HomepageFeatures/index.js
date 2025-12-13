import clsx from "clsx";
import Heading from "@theme/Heading";
import styles from "./styles.module.css";

const FeatureList = [
  {
    title: "In-Depth Network Analysis",
    Svg: require("@site/static/img/undraw_connected_world_wuay.svg").default,
    description: (
      <>
        Analyze your network and perform in-depth diagnostics using built-in
        tools such as WiFi Analyzer, IP Scanner, Port Scanner, Traceroute,
        DNS Lookup, Ping Monitor, LLDP Capture, and many more - all in a unified
        interface.
      </>
    ),
  },
  {
    title: "Remote Access & Server Management",
    Svg: require("@site/static/img/undraw_server_cluster_jwwq.svg").default,
    description: (
      <>
        Connect, monitor and troubleshoot your network and server
        infrastructure with integrated clients such as Remote Desktop (RDP),
        PuTTY (SSH, Serial, etc.), PowerShell (WSL, K9s, etc.)
        and TigerVNC (VNC).
      </>
    ),
  },
  {
    title: "Profile Management",
    Svg: require("@site/static/img/undraw_personal_settings_re_i6w4.svg")
      .default,
    description: (
      <>
        Organize hosts and networks in profiles with specific configurations,
        and use them seamlessly across all features. Keep your sensitive
        data secure with encrypted profile files, and manage customers
        or environments by using different profile files.
      </>
    ),
  },
  {
    title: "Effortless Troubleshooting",
    Svg: require("@site/static/img/undraw_server_down_s-4-lk.svg").default,
    description: (
      <>
        Diagnose and resolve issues effectively with a comprehensive suite of
        tools in one place. It's enterprise-ready — includes an MSI installer for
        centralized deployment, signed binaries, translations in 16+ languages,
        and distribution via package managers like winget and Chocolatey.
      </>
    ),
  },  
  {
    title: "Open Source",
    Svg: require("@site/static/img/undraw_version_control_re_mg66.svg").default,
    description: (
      <>
        NETworkManager is fully open source on GitHub — no ads, no subscriptions,
        no selling your data. Review the code, build it yourself, or contribute
        to make it even better.
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
