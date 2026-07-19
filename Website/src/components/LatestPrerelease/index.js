import { useState } from "react";
import prerelease from "@site/src/data/prerelease.json";
import ReleaseHero from "@site/src/components/ReleaseHero";
import ArchSwitcher from "@site/src/components/ArchSwitcher";
import DownloadCard, { DownloadGrid } from "@site/src/components/DownloadCard";

// Pre-release data is prefetched at build time (see scripts/fetch-release-data.mjs)
// and read statically here — no client-side GitHub request and no caching.
const VARIANTS = [
  { key: "setup", icon: "📦", label: "Setup", sub: "MSI Installer", color: "primary" },
  { key: "portable", icon: "🎒", label: "Portable", sub: "ZIP Archive", color: "info" },
  { key: "archive", icon: "🗄️", label: "Archive", sub: "ZIP Archive", color: "info" },
];

const ARCH_LABELS = { x64: "x64", arm64: "ARM64" };

export default function LatestPrerelease() {
  // Only architectures that actually shipped assets for this release (older
  // releases predate ARM64 and will only have "x64").
  const architectures = prerelease.available
    ? Object.keys(prerelease.downloads ?? {})
    : [];
  const [arch, setArch] = useState(architectures[0]);

  if (!prerelease.available) {
    return (
      <p>
        <em>Currently no pre-release version available.</em>
      </p>
    );
  }

  const { version, releaseDate, changelog, downloads, checksums } = prerelease;
  const archDownloads = downloads[arch] ?? {};
  const archChecksums = checksums?.[arch];

  return (
    <>
      <ReleaseHero
        compact
        tag="Pre-release"
        version={version}
        releaseDate={releaseDate}
        changelog={changelog}
        changelogLabel="🚀 What's coming?"
      />

      <div className="margin-bottom--lg">
        {architectures.length > 1 && (
          <div
            style={{
              display: "flex",
              justifyContent: "center",
              marginBottom: "16px",
            }}
          >
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
              downloadUrl={archDownloads[v.key]}
              checksum={archChecksums?.[v.key]}
            />
          ))}
        </DownloadGrid>
      </div>
    </>
  );
}
