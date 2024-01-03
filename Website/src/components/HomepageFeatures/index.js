import clsx from "clsx";
import Heading from "@theme/Heading";
import styles from "./styles.module.css";

const FeatureList = [
  {
    title: "Analyze Networks",
    Svg: require("@site/static/img/undraw_connected_world_wuay.svg").default,
    description: (
      <>
        Analyze your network and get detailed information about hosts with the
        numerous built-in features such as IP / port scanner, traceroute, DNS
        query and much more.
      </>
    ),
  },
  {
    title: "Manage Systems",
    Svg: require("@site/static/img/undraw_server_cluster_jwwq.svg").default,
    description: (
      <>
        Manage your network and server infrastructure via various protocols such
        as SSH, RDP, VNC, WinRM or AWS SSM with the integrated clients.
      </>
    ),
  },
  {
    title: "Troubleshoot Problems",
    Svg: require("@site/static/img/undraw_server_down_s-4-lk.svg").default,
    description: (
      <>
        Effectively analyze and resolve problems by combining numerous features
        in a single application.
      </>
    ),
  },
  {
    title: "Profiles Management",
    Svg: require("@site/static/img/undraw_personal_settings_re_i6w4.svg")
      .default,
    description: (
      <>
        Save your hosts with specific configurations in profiles and use them
        across all features. Seperate them by environments and encrypt them to
        protect your sensitive data.
      </>
    ),
  },
  {
    title: "Open Source",
    Svg: require("@site/static/img/undraw_version_control_re_mg66.svg").default,
    description: (
      <>
        NETworkManager is open source and available on GitHub! You can review
        the code at any time, build it yourself and even contribute to the
        project to improve it.
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
