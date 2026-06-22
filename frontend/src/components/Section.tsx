import type { ReactNode } from "react";
import styles from "./Section.module.css";

// A collapsible <details> section with an optional count pill in the summary.
export default function Section({
  title,
  count,
  defaultOpen = true,
  children,
}: {
  title: string;
  count?: number;
  defaultOpen?: boolean;
  children: ReactNode;
}) {
  return (
    <details className={styles.section} open={defaultOpen}>
      <summary className={styles.summary}>
        {title}
        {count !== undefined && <span className={styles.pill}>{count}</span>}
      </summary>
      {children}
    </details>
  );
}
