"use client";

import { CheckboxField, NumberField } from "@/components/fields/NumberField";
import type { Limits, RenderSettings } from "@/lib/types";
import styles from "./panels.module.css";

export default function RenderSettingsPanel({
  value,
  limits,
  onChange,
}: {
  value: RenderSettings;
  limits: Limits;
  onChange: (patch: Partial<RenderSettings>) => void;
}) {
  return (
    <div className={styles.grid}>
      <NumberField
        label={`Width (max ${limits.maxWidth})`}
        value={value.width}
        onChange={(width) => onChange({ width })}
        step="1"
      />
      <NumberField
        label={`Height (max ${limits.maxHeight})`}
        value={value.height}
        onChange={(height) => onChange({ height })}
        step="1"
      />
      <NumberField
        label={`Anti-aliasing (max ${limits.maxAaMultiplier})`}
        value={value.aaMultiplier}
        onChange={(aaMultiplier) => onChange({ aaMultiplier })}
        step="1"
      />
      <CheckboxField
        label="Ambient lighting"
        value={value.ambientLighting}
        onChange={(ambientLighting) => onChange({ ambientLighting })}
      />
    </div>
  );
}
