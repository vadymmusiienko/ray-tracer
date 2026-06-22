"use client";

/* eslint-disable @next/next/no-img-element */
import styles from "./Preview.module.css";

export interface PreviewState {
  imageUrl: string | null;
  status: string;
  busy: boolean;
  errors: string[];
}

// The right-hand pane: status line, validation errors, and the rendered PNG.
// Uses a plain <img> (not next/image) because the source is a runtime object
// URL of a freshly rendered blob, not a build-time static asset.
export default function Preview({ state }: { state: PreviewState }) {
  const { imageUrl, status, busy, errors } = state;

  return (
    <section className={styles.preview}>
      <div className={styles.inner}>
        <div className={`${styles.status}${busy ? ` ${styles.busy}` : ""}`}>
          {status}
        </div>

        {errors.length > 0 && (
          <div className={styles.errors}>
            <strong>Could not render:</strong>
            <ul>
              {errors.map((e, i) => (
                <li key={i}>{e}</li>
              ))}
            </ul>
          </div>
        )}

        <div className={styles.frame}>
          {imageUrl ? (
            <img className={styles.image} src={imageUrl} alt="Rendered scene" />
          ) : (
            <p className={styles.placeholder}>
              Build a scene and hit <strong>Render</strong>.
            </p>
          )}
        </div>
      </div>
    </section>
  );
}
