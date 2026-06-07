import clsx from "clsx";
import Heading from "@theme/Heading";
import styles from "./styles.module.css";

const FeatureList = [
  {
    title: "One Place for Everything",
    Svg: require("@site/static/img/undraw-connected-world-wuay.svg").default,
    description: (
      <>
        Stop switching between a dozen utilities. Diagnose, connect, and manage
        your entire network from a single unified interface.
      </>
    ),
  },
  {
    title: "Full Remote Access Built In",
    Svg: require("@site/static/img/undraw-server-cluster-jwwq.svg").default,
    description: (
      <>
        Connect to desktops, servers, and containers directly from the app.
        No separate clients to install or maintain.
      </>
    ),
  },
  {
    title: "Profiles That Work Everywhere",
    Svg: require("@site/static/img/undraw-personal-settings-re-i6w4.svg")
      .default,
    description: (
      <>
        Organize hosts by customer or environment. Every tool opens
        pre-configured — nothing to re-enter, ever.
      </>
    ),
  },
  {
    title: "Enterprise-Ready",
    Svg: require("@site/static/img/undraw-server-down-s-4-lk.svg").default,
    description: (
      <>
        MSI installer, signed binaries, Group Policy, package manager support,
        and Active Directory import. Deploy it to your whole team in minutes.
      </>
    ),
  },
  {
    title: "Free. Open Source.",
    Svg: require("@site/static/img/undraw-version-control-re-mg66.svg").default,
    description: (
      <>
        No license, no subscription, no telemetry. The source is on
        GitHub — read it, audit it, or contribute to it.
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
