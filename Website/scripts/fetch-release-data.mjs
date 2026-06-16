// Pre-fetches release data at build time and writes it to src/data/*.json:
//   - release.json    : current stable release    (version, date, downloads, checksums)
//   - prerelease.json : latest pre-release         (version, date, downloads, checksums)
//
// Both files use the exact same shape and are built the same way — there is
// nothing to bump by hand. The committed JSON files only act as an offline
// fallback (last known values); every real build overwrites them with fresh data.
//
// Why build-time instead of client-side:
//   - The GitHub API (api.github.com) supports CORS but is rate limited
//     (60 requests/hour per IP for unauthenticated users) — doing it per visitor
//     risks hitting that limit.
//   - The release asset host (release-assets.githubusercontent.com) is NOT rate
//     limited but sends no Access-Control-Allow-Origin header, so a browser
//     fetch() of the SHA256SUMS file is blocked by CORS.
//   - Node has no CORS restriction and runs once per build, so fetching here is
//     reliable, includes the checksums, and adds zero per-visitor requests.
//
// Freshness: the website is rebuilt on every published (pre-)release (see the
// `release` trigger in .github/workflows/deploy_website.yml), so the prefetched
// data stays current without any client-side requests.
import { writeFile } from "node:fs/promises";
import { existsSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";

const __dirname = dirname(fileURLToPath(import.meta.url));
const dataDir = resolve(__dirname, "..", "src", "data");
const changelogDir = resolve(__dirname, "..", "docs", "changelog");
const releasePath = resolve(dataDir, "release.json");
const prereleasePath = resolve(dataDir, "prerelease.json");

const REPO = "BornToBeRoot/NETworkManager";

// Changelog category index — used when a release has no dedicated changelog page
// yet, so the link never points at a non-existent doc (onBrokenLinks: "throw").
const CHANGELOG_FALLBACK = "/docs/category/changelog";

// Maps the data keys to the asset filename suffixes.
const ASSET_SUFFIXES = {
  setup: "_Setup.msi",
  portable: "_Portable.zip",
  archive: "_Archive.zip",
};

function ghHeaders() {
  const headers = {
    "User-Agent": "NETworkManager-website-build",
    Accept: "application/vnd.github+json",
  };
  // Authenticate when a token is available (CI) to raise the rate limit.
  const token = process.env.GITHUB_TOKEN;
  if (token) headers.Authorization = `Bearer ${token}`;
  return headers;
}

function formatDate(iso) {
  const d = new Date(iso);
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  return `${dd}.${mm}.${d.getFullYear()}`;
}

async function writeJson(path, data) {
  await writeFile(path, JSON.stringify(data, null, 2) + "\n");
}

// Downloads and parses the "<hash>  <filename>" SHA256SUMS asset into a
// { setup, portable, archive } map. Throws if any variant is missing.
async function fetchChecksumMap(version) {
  const url = `https://github.com/${REPO}/releases/download/${version}/NETworkManager_${version}_SHA256SUMS`;
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status} for ${url}`);
  const text = await res.text();

  const map = {};
  for (const line of text.split(/\r?\n/)) {
    const m = line.trim().match(/^([0-9a-fA-F]{64})\s+(.+)$/);
    if (!m) continue;
    for (const [key, suffix] of Object.entries(ASSET_SUFFIXES)) {
      if (m[2] === `NETworkManager_${version}${suffix}`) {
        map[key] = m[1].toUpperCase();
      }
    }
  }
  for (const key of Object.keys(ASSET_SUFFIXES)) {
    if (!map[key]) throw new Error(`Missing ${key} checksum for ${version}`);
  }
  return map;
}

// Resolves the changelog link for a stable version: its dedicated page if the
// doc exists ("2026.2.22.0" -> /docs/changelog/2026-2-22-0), else the fallback.
function stableChangelogUrl(version) {
  const slug = version.replace(/\./g, "-");
  return existsSync(resolve(changelogDir, `${slug}.md`))
    ? `/docs/changelog/${slug}`
    : CHANGELOG_FALLBACK;
}

// Builds the unified data object (download URLs + checksums + changelog link)
// from a GitHub release object. Used identically for both stable and pre-release.
async function buildData(release, changelog) {
  const version = release.tag_name;

  const downloads = {};
  for (const [key, suffix] of Object.entries(ASSET_SUFFIXES)) {
    const asset = release.assets.find((a) => a.name.endsWith(suffix));
    if (asset) downloads[key] = asset.browser_download_url;
  }

  // Checksums are best-effort — older releases may not ship a SHA256SUMS file.
  let checksums = null;
  try {
    checksums = await fetchChecksumMap(version);
  } catch (err) {
    const msg = err instanceof Error ? err.message : String(err);
    console.warn(`[release-data] Checksums unavailable for ${version}: ${msg}`);
  }

  return {
    available: true,
    version,
    releaseDate: formatDate(release.published_at),
    changelog,
    downloads,
    checksums,
  };
}

async function updateStable(releases) {
  // Newest published, non-draft, non-pre-release.
  const stable = releases.find((r) => !r.draft && !r.prerelease);
  if (!stable) throw new Error("No stable release found");
  await writeJson(
    releasePath,
    await buildData(stable, stableChangelogUrl(stable.tag_name)),
  );
  console.log(`[release-data] Stable updated to ${stable.tag_name}`);
}

async function updatePrerelease(releases) {
  // The newest non-draft release; only show it if it is actually a pre-release
  // (i.e. newer than the latest stable). Otherwise the section is hidden.
  const latest = releases.find((r) => !r.draft);
  if (!latest || !latest.prerelease) {
    await writeJson(prereleasePath, { available: false });
    console.log("[release-data] No pre-release available");
    return;
  }
  // Pre-release changes are documented on the rolling "next release" page.
  await writeJson(
    prereleasePath,
    await buildData(latest, "/docs/changelog/next-release"),
  );
  console.log(`[release-data] Pre-release updated to ${latest.tag_name}`);
}

async function main() {
  const apiUrl = `https://api.github.com/repos/${REPO}/releases?per_page=30`;
  const res = await fetch(apiUrl, { headers: ghHeaders() });
  if (!res.ok) throw new Error(`HTTP ${res.status} for ${apiUrl}`);
  const releases = await res.json();

  // Each step is independent so a single failure keeps that file's committed
  // fallback instead of failing the whole build.
  for (const [label, fn] of [
    ["stable", updateStable],
    ["pre-release", updatePrerelease],
  ]) {
    try {
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      console.warn(`[release-data] ${label} skipped: ${msg}`);
    }
  }
}

main().catch((err) => {
  // Non-fatal: keep all committed fallbacks so offline / CI builds still succeed.
  const msg = err instanceof Error ? err.message : String(err);
  console.warn(`[release-data] Skipped update: ${msg}`);
});
