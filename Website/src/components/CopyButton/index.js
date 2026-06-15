import { useState } from "react";
import clsx from "clsx";
import styles from "./styles.module.css";

// Copies a value to the clipboard with brief visual feedback. Used for the
// SHA-256 checksums on the download cards — the hash itself is never rendered,
// which keeps the cards compact and mobile-safe.
export default function CopyButton({
  value,
  idleLabel = "📋 Copy SHA-256",
  copiedLabel = "✅ Copied",
  className,
}) {
  const [copied, setCopied] = useState(false);

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(value);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // Clipboard API unavailable (e.g. insecure context) — silently ignore.
    }
  }

  return (
    <button
      type="button"
      className={clsx(styles.copyButton, className)}
      onClick={handleCopy}
      aria-label="Copy SHA-256 checksum to clipboard"
    >
      {copied ? copiedLabel : idleLabel}
    </button>
  );
}
