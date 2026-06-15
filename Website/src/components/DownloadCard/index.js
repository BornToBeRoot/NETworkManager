import Link from "@docusaurus/Link";
import clsx from "clsx";
import CopyButton from "@site/src/components/CopyButton";
import styles from "./styles.module.css";

// Responsive grid wrapper for a row of <DownloadCard> elements.
export function DownloadGrid({ children }) {
  return <div className={styles.grid}>{children}</div>;
}

// A single download option. `description` and `recommended` are optional, so the
// same card renders both the detailed stable variant and the compact
// pre-release variant. When a `checksum` is given, a copy button is shown.
export default function DownloadCard({
  icon,
  label,
  sub,
  color = "primary",
  description,
  recommended = false,
  downloadUrl,
  checksum,
}) {
  return (
    <div className={styles.card}>
      {recommended && <span className={styles.badge}>★ Recommended</span>}
      <div className={styles.icon} aria-hidden="true">
        {icon}
      </div>
      <div className={styles.title}>{label}</div>
      <div className={styles.sub}>{sub}</div>
      {description && <p className={styles.description}>{description}</p>}
      <Link
        className={clsx("button", `button--${color}`, styles.button)}
        to={downloadUrl}
      >
        ⬇ Download
      </Link>
      {checksum && <CopyButton value={checksum} />}
    </div>
  );
}
