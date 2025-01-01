import clsx from "clsx";
import Heading from "@theme/Heading";
import styles from "./styles.module.css";

const FeatureList = [
  {
    title: "Network Analysis",
    Svg: require("@site/static/img/undraw_connected_world_wuay.svg").default,
    description: (
      <>
        Analyze your network and gather detailed information using built-in
        tools such as the WiFi Analyzer, IP Scanner, Port Scanner, Traceroute,
        DNS Lookup, Ping Monitor, LLDP/CDP Capture, and many [more(./docs/features)].
      </>
    ),
  },
  {
    title: "Remote System Management",
    Svg: require("@site/static/img/undraw_server_cluster_jwwq.svg").default,
    description: (
      <>
        Connect to remote systems and manage your network and server
        infrastructure with integrated clients such as Remote Desktop (RDP),
        PuTTY (SSH, Telnet, Serial), PowerShell (WinRM), TigerVNC (VNC), and AWS
        Console (AWS SSM).
      </>
    ),
  },
  {
    title: "Effortless Troubleshooting",
    Svg: require("@site/static/img/undraw_server_down_s-4-lk.svg").default,
    description: (
      <>
        Diagnose and resolve issues effectively with a comprehensive suite of
        tools within a unified application.
      </>
    ),
  },
  {
    title: "Profile Management",
    Svg: require("@site/static/img/undraw_personal_settings_re_i6w4.svg")
      .default,
    description: (
      <>
        Save hosts and networks with custom configurations in encrypted profiles
        to protect your sensitive data, organize them by environment (e.g.,
        Company A, Company B), and use them seamlessly across features.
      </>
    ),
  },
  {
    title: "Open Source",
    Svg: require("@site/static/img/undraw_version_control_re_mg66.svg").default,
    description: (
      <>
        NETworkManager is fully open source on GitHub. Review the code, build it
        yourself, or contribute to make it even better.
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
