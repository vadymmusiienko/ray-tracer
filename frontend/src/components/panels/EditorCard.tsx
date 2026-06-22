import type { ReactNode } from "react";
import styles from "./panels.module.css";

// A removable card wrapping one material/light/object's fields.
export default function EditorCard({
  tag,
  onRemove,
  children,
}: {
  tag: string;
  onRemove: () => void;
  children: ReactNode;
}) {
  return (
    <div className={styles.card}>
      <div className={styles.cardHead}>
        <span className={styles.tag}>{tag}</span>
        <button className={styles.remove} title="Remove" onClick={onRemove}>
          ✕
        </button>
      </div>
      <div className={styles.grid}>{children}</div>
    </div>
  );
}
