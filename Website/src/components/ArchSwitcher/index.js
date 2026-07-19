import clsx from "clsx";
import styles from "./styles.module.css";

// Segmented control to pick which CPU architecture's downloads are shown.
// The caller is responsible for hiding it when only one architecture is available.
export default function ArchSwitcher({ options, value, onChange, className }) {
  return (
    <div
      className={clsx(styles.switcher, className)}
      role="tablist"
      aria-label="Architecture"
    >
      {options.map((option) => (
        <button
          key={option.key}
          type="button"
          role="tab"
          aria-selected={option.key === value}
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
