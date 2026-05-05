import { useEffect, useState } from "react";
import Link from "@docusaurus/Link";
import pageStyles from "../../pages/styles.module.css";

const API_URL =
  "https://api.github.com/repos/BornToBeRoot/NETworkManager/releases?per_page=30";
const CACHE_KEY = "ntm-latest-prerelease";
const CACHE_TTL_MS = 10 * 60 * 1000;

const DOWNLOAD_VARIANTS = [
  { field: "setupUrl", label: "Setup", color: "primary" },
  { field: "portableUrl", label: "Portable", color: "info" },
  { field: "archiveUrl", label: "Archive", color: "info" },
];

function readCache() {
  try {
    const raw = localStorage.getItem(CACHE_KEY);
    if (!raw) return null;
    const { timestamp, data } = JSON.parse(raw);
    if (Date.now() - timestamp > CACHE_TTL_MS) return null;
    return data;
  } catch {
    return null;
  }
}

function writeCache(data) {
  try {
    localStorage.setItem(
      CACHE_KEY,
      JSON.stringify({ timestamp: Date.now(), data }),
    );
  } catch {
    // ignore quota / unavailable storage
  }
}

function findAssetUrl(assets, suffix) {
  return assets.find((a) => a.name.endsWith(suffix))?.browser_download_url;
}

function formatReleaseDate(iso) {
  const d = new Date(iso);
  const dd = String(d.getDate()).padStart(2, "0");
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  return `${dd}.${mm}.${d.getFullYear()}`;
}

export default function LatestPrerelease() {
  const [state, setState] = useState({ status: "loading" });

  useEffect(() => {
    const cached = readCache();
    if (cached !== null) {
      setState(
        cached.release
          ? { status: "ready", release: cached.release }
          : { status: "none" },
      );
      return;
    }

    const controller = new AbortController();
    const { signal } = controller;

    fetch(API_URL, { signal })
      .then((res) => {
        if (!res.ok) throw new Error(`GitHub API ${res.status}`);
        return res.json();
      })
      .then((releases) => {
        if (signal.aborted) return;
        const latest = releases.find((r) => !r.draft);
        const pre = latest && latest.prerelease ? latest : null;
        if (!pre) {
          writeCache({ release: null });
          setState({ status: "none" });
          return;
        }
        const release = {
          tagName: pre.tag_name,
          publishedAt: pre.published_at,
          htmlUrl: pre.html_url,
          setupUrl: findAssetUrl(pre.assets, "_Setup.msi"),
          portableUrl: findAssetUrl(pre.assets, "_Portable.zip"),
          archiveUrl: findAssetUrl(pre.assets, "_Archive.zip"),
        };
        writeCache({ release });
        setState({ status: "ready", release });
      })
      .catch((err) => {
        if (err.name === "AbortError" || signal.aborted) return;
        setState({ status: "error" });
      });

    return () => controller.abort();
  }, []);

  if (state.status === "none") {
    return (
      <p>
        <em>Currently no pre-release version available.</em>
      </p>
    );
  }

  if (state.status === "error") {
    return (
      <p>
        <em>
          Pre-release info couldn't be loaded right now. See{" "}
          <Link to="https://github.com/BornToBeRoot/NETworkManager/releases">
            GitHub Releases
          </Link>
          .
        </em>
      </p>
    );
  }

  if (state.status !== "ready") {
    return null;
  }

  const { release } = state;
  const downloads = DOWNLOAD_VARIANTS.filter((v) => release[v.field]);

  return (
    <>
      <div style={{ display: "flex", justifyContent: "space-between" }}>
        <h3>{release.tagName}</h3>
        <div>Release Date: {formatReleaseDate(release.publishedAt)}</div>
      </div>

      <div className={`${pageStyles.centerButtonContainer} margin-bottom--lg`}>
        {downloads.map((v) => (
          <Link
            key={v.field}
            className={`button button--${v.color} button--outline ${pageStyles.downloadButton}`}
            to={release[v.field]}
          >
            {v.label}
          </Link>
        ))}
      </div>

      <div className={pageStyles.centerButtonContainer}>
        <Link
          className={`button button--secondary button--outline ${pageStyles.additionalButton}`}
          to={release.htmlUrl}
          target="_blank"
        >
          📃 Release notes
        </Link>
      </div>
    </>
  );
}
