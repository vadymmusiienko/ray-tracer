"use client";

import { NumberField, Vec3Field } from "@/components/fields/NumberField";
import type { Camera } from "@/lib/types";
import styles from "./panels.module.css";

export default function CameraPanel({
  value,
  onChange,
}: {
  value: Camera;
  onChange: (patch: Partial<Camera>) => void;
}) {
  return (
    <div className={styles.grid}>
      <Vec3Field
        label="Position (x, y, z)"
        value={value.position}
        onChange={(position) => onChange({ position })}
      />
      <Vec3Field
        label="Rotation axis (x, y, z)"
        value={value.rotationAxis}
        onChange={(rotationAxis) => onChange({ rotationAxis })}
      />
      <NumberField
        label="Rotation angle (°)"
        value={value.rotationAngle}
        onChange={(rotationAngle) => onChange({ rotationAngle })}
      />
      <NumberField
        label="Scale"
        value={value.scale}
        onChange={(scale) => onChange({ scale })}
      />
    </div>
  );
}
