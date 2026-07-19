import { useState } from "react";
import release from "@site/src/data/release.json";
import ReleaseHero from "@site/src/components/ReleaseHero";
import ArchSwitcher from "@site/src/components/ArchSwitcher";
import DownloadCard, { DownloadGrid } from "@site/src/components/DownloadCard";
import styles from "./styles.module.css";

const VARIANTS = [
  {
    key: "setup",
    icon: "📦",
    label: "Setup",
    sub: "MSI Installer",
    recommended: true,
    description:
      "Installs system-wide and requires administrator privileges. The best choice for most users and ideal for centralized deployment via Intune, SCCM, etc.",
    color: "primary",
  },
  {
    key: "portable",
    icon: "🎒",
    label: "Portable",
    sub: "ZIP Archive",
    description:
      "Run without installation — ideal for a USB stick or network share. Settings and profiles are stored next to the executable.",
    color: "info",
  },
  {
    key: "archive",
    icon: "🗄️",
    label: "Archive",
    sub: "ZIP Archive",
    description:
      "Like Portable, but stores settings and profiles in your Documents folder. Useful for custom deployment scenarios (e.g. Terminal Server).",
    color: "info",
  },
];

const ARCH_LABELS = { x64: "x64", arm64: "ARM64" };

export default function DownloadSection() {
  const { version, releaseDate, changelog, downloads, checksums } = release;

  // Only architectures that actually shipped assets for this release (older
  // releases predate ARM64 and will only have "x64").
  const architectures = Object.keys(downloads);
  const [arch, setArch] = useState(architectures[0]);

  const archDownloads = downloads[arch] ?? {};
  const archChecksums = checksums?.[arch];

  return (
    <div className={styles.downloadSection}>
      <ReleaseHero
        version={version}
        releaseDate={releaseDate}
        changelog={changelog}
        changelogLabel="✨ What's new?"
      />

      {architectures.length > 1 && (
        <div className={styles.archSwitcherRow}>
          <ArchSwitcher
            options={architectures.map((a) => ({
              key: a,
              label: ARCH_LABELS[a] ?? a,
            }))}
            value={arch}
            onChange={setArch}
          />
        </div>
      )}

      <DownloadGrid>
        {VARIANTS.filter((v) => archDownloads[v.key]).map((v) => (
          <DownloadCard
            key={v.key}
            icon={v.icon}
            label={v.label}
            sub={v.sub}
            color={v.color}
            description={v.description}
            recommended={v.recommended}
            downloadUrl={archDownloads[v.key]}
            checksum={archChecksums?.[v.key]}
          />
        ))}
      </DownloadGrid>
    </div>
  );
}
