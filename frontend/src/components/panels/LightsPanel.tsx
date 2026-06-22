"use client";

import { ColorField, Vec3Field } from "@/components/fields/NumberField";
import type { LightForm } from "@/lib/types";
import EditorCard from "./EditorCard";
import styles from "./panels.module.css";

export default function LightsPanel({
  lights,
  onPatch,
  onAdd,
  onRemove,
}: {
  lights: LightForm[];
  onPatch: (key: string, patch: Partial<LightForm>) => void;
  onAdd: () => void;
  onRemove: (key: string) => void;
}) {
  return (
    <>
      <div className={styles.list}>
        {lights.map((l) => (
          <EditorCard
            key={l.key}
            tag="Point light"
            onRemove={() => onRemove(l.key)}
          >
            <Vec3Field
              label="Position (x, y, z)"
              value={l.position}
              onChange={(position) => onPatch(l.key, { position })}
            />
            <ColorField
              label="Color"
              value={l.color}
              onChange={(color) => onPatch(l.key, { color })}
            />
          </EditorCard>
        ))}
      </div>
      <button className={styles.add} onClick={onAdd}>
        + Add light
      </button>
    </>
  );
}
