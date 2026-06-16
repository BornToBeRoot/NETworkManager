import styles from "./styles.module.css";

// Responsive comparison of the download variants. Renders as three columns on
// desktop and stacks vertically on mobile — unlike a markdown table, it never
// overflows the viewport and so never causes horizontal page scrolling.
const VARIANTS = [
  {
    version: "Setup (MSI)",
    description: (
      <>
        Installs system-wide and requires administrator privileges, integrating
        with Windows like any other installed app. It's the right choice for most
        users and the standard way to roll NETworkManager out to many machines via
        Intune, SCCM, or Group Policy (see <em>Silent install</em> below). Updates
        install cleanly over a previous version.
      </>
    ),
    location: <code>~\Documents\NETworkManager</code>,
  },
  {
    version: "Portable (ZIP)",
    description: (
      <>
        Runs straight from the extracted folder — no installation and no
        administrator rights required, which makes it ideal for a USB stick, a
        network share, or locked-down environments. Settings and profiles are
        stored next to the executable, so the whole setup travels with the folder.
        To upgrade, copy your <code>Profiles</code> and <code>Settings</code>{" "}
        directories into the new version.
      </>
    ),
    location: "Next to the executable",
  },
  {
    version: "Archive (ZIP)",
    description: (
      <>
        Works like the Portable version — no installation required — but stores
        settings and profiles in your Documents folder instead of next to the
        executable. This suits custom or multi-user setups such as a Terminal
        Server, where the application files stay shared while each user keeps their
        own profile. Just extract it and run the executable directly.
      </>
    ),
    location: <code>~\Documents\NETworkManager</code>,
  },
];

export default function VersionComparison() {
  return (
    <div className={styles.grid}>
      {VARIANTS.map((v) => (
        <div key={v.version} className={styles.card}>
          <div className={styles.cardTitle}>{v.version}</div>
          <p className={styles.cardDesc}>{v.description}</p>
          <div className={styles.cardSpec}>
            <span className={styles.specLabel}>Settings &amp; profiles</span>
            <span>{v.location}</span>
          </div>
        </div>
      ))}
    </div>
  );
}
