import { useEffect, useState } from "react";
import clsx from "clsx";
import styles from "./styles.module.css";

// Selected architecture of a download section: the first one by default, or ARM64
// if the browser reports an ARM device (Chromium-based browsers, e.g. Edge on
// Windows on ARM). Detected after the first render, so the static HTML stays the
// same for everyone; other browsers keep the default.
export function useArchitecture(architectures) {
  const [arch, setArch] = useState(architectures[0]);

  useEffect(() => {
    if (!architectures.includes("arm64")) return;

    navigator.userAgentData
      ?.getHighEntropyValues?.(["architecture"])
      .then(({ architecture }) => {
        if (architecture === "arm") setArch("arm64");
      })
      .catch(() => {
        // Not available - keep the default
      });
    // Only on mount, the architectures of a release don't change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return [arch, setArch];
}

// Segmented control to pick which CPU architecture's downloads are shown.
// Always rendered, even with a single option, so the user can see which
// architecture the download links point to.
export default function ArchSwitcher({ options, value, onChange, className }) {
  return (
    <div
      className={clsx(styles.switcher, className)}
      role="group"
      aria-label="Architecture"
    >
      {options.map((option) => (
        <button
          key={option.key}
          type="button"
          aria-pressed={option.key === value}
          className={clsx(
            styles.option,
            option.key === value && styles.optionActive,
          )}
          onClick={() => onChange(option.key)}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}
