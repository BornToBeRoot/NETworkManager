import Link from "@docusaurus/Link";
import tools from "@site/src/data/tools";
import styles from "./styles.module.css";

// Compact, clickable chip grid of all built-in tools. Shared by the homepage
// and the docs introduction so both stay in sync with src/data/tools.js.
export default function ToolChips() {
  return (
    <div className={styles.toolChips}>
      {tools.map((tool) => (
        <Link
          key={tool.name}
          to={tool.to}
          className={styles.toolChip}
          title={tool.note}
        >
          {tool.name}
        </Link>
      ))}
    </div>
  );
}
