import prerelease from "@site/src/data/prerelease.json";
import ReleaseHero from "@site/src/components/ReleaseHero";
import DownloadCard, { DownloadGrid } from "@site/src/components/DownloadCard";

// Pre-release data is prefetched at build time (see scripts/fetch-release-data.mjs)
// and read statically here — no client-side GitHub request and no caching.
const VARIANTS = [
  { key: "setup", icon: "📦", label: "Setup", sub: "MSI Installer", color: "primary" },
  { key: "portable", icon: "🎒", label: "Portable", sub: "ZIP Archive", color: "info" },
  { key: "archive", icon: "🗄️", label: "Archive", sub: "ZIP Archive", color: "info" },
];

export default function LatestPrerelease() {
  if (!prerelease.available) {
    return (
      <p>
        <em>Currently no pre-release version available.</em>
      </p>
    );
  }

  const { version, releaseDate, changelog, downloads, checksums } = prerelease;

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
        <DownloadGrid>
          {VARIANTS.filter((v) => downloads[v.key]).map((v) => (
            <DownloadCard
              key={v.key}
              icon={v.icon}
              label={v.label}
              sub={v.sub}
              color={v.color}
              downloadUrl={downloads[v.key]}
              checksum={checksums?.[v.key]}
            />
          ))}
        </DownloadGrid>
      </div>
    </>
  );
}
