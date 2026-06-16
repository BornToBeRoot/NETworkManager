import Link from "@docusaurus/Link";
import clsx from "clsx";
import styles from "./styles.module.css";

// Dark banner showing a release version + date and a changelog link. Used for
// both the stable download (full size) and the pre-release (compact, tagged).
export default function ReleaseHero({
  version,
  releaseDate,
  changelog,
  changelogLabel,
  tag,
  compact = false,
}) {
  return (
    <div className={clsx(styles.hero, compact && styles.heroCompact)}>
      <div className={styles.heroInfo}>
        <div className={styles.heroVersionRow}>
          <span className={styles.heroVersion}>{version}</span>
          {tag && <span className={styles.heroTag}>{tag}</span>}
        </div>
        <span className={styles.heroDate}>Released {releaseDate}</span>
      </div>
      {changelog && (
        <Link className={styles.heroChangelog} to={changelog}>
          {changelogLabel}
        </Link>
      )}
    </div>
  );
}
