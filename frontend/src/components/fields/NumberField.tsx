"use client";

import { useEffect, useRef, useState } from "react";
import type { ReactNode } from "react";
import { hexToRgb, rgbToHex } from "@/lib/colors";
import type { Vec3 } from "@/lib/types";
import styles from "./fields.module.css";

// ---------------------------------------------------------------------------
// Small controlled field primitives shared by every panel.
// ---------------------------------------------------------------------------

export function Field({
  label,
  span,
  children,
}: {
  label: string;
  span?: boolean;
  children: ReactNode;
}) {
  return (
    <div className={`${styles.field}${span ? ` ${styles.span2}` : ""}`}>
      <label>{label}</label>
      {children}
    </div>
  );
}

// A numeric <input> that keeps a local text buffer so intermediate states
// ("-", "1.", "") are typable, only emitting parsed finite numbers upstream.
export function NumberInput({
  value,
  onChange,
  step = "any",
}: {
  value: number;
  onChange: (value: number) => void;
  step?: string;
}) {
  const [text, setText] = useState(String(value));
  const focused = useRef(false);

  // Sync from the prop only while not actively editing (e.g. loading a scene).
  useEffect(() => {
    if (!focused.current) setText(String(value));
  }, [value]);

  return (
    <input
      type="number"
      step={step}
      value={text}
      onFocus={() => (focused.current = true)}
      onBlur={() => {
        focused.current = false;
        setText(String(value));
      }}
      onChange={(e) => {
        setText(e.target.value);
        const parsed = parseFloat(e.target.value);
        if (Number.isFinite(parsed)) onChange(parsed);
      }}
    />
  );
}

export function NumberField({
  label,
  value,
  onChange,
  step,
}: {
  label: string;
  value: number;
  onChange: (value: number) => void;
  step?: string;
}) {
  return (
    <Field label={label}>
      <NumberInput value={value} onChange={onChange} step={step} />
    </Field>
  );
}

export function Vec3Field({
  label,
  value,
  onChange,
}: {
  label: string;
  value: Vec3;
  onChange: (value: Vec3) => void;
}) {
  return (
    <Field label={label} span>
      <div className={styles.vec3}>
        {([0, 1, 2] as const).map((i) => (
          <NumberInput
            key={i}
            value={value[i]}
            onChange={(v) => {
              const next: Vec3 = [...value];
              next[i] = v;
              onChange(next);
            }}
          />
        ))}
      </div>
    </Field>
  );
}

export function ColorField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: Vec3;
  onChange: (value: Vec3) => void;
}) {
  return (
    <Field label={label}>
      <input
        type="color"
        value={rgbToHex(value)}
        onChange={(e) => onChange(hexToRgb(e.target.value))}
      />
    </Field>
  );
}

export function TextField({
  label,
  value,
  onChange,
  onCommit,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  onCommit?: () => void;
}) {
  return (
    <Field label={label}>
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        onBlur={onCommit}
      />
    </Field>
  );
}

export function SelectField({
  label,
  options,
  value,
  onChange,
}: {
  label: string;
  options: readonly string[];
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <Field label={label}>
      <select value={value} onChange={(e) => onChange(e.target.value)}>
        {options.map((o) => (
          <option key={o} value={o}>
            {o}
          </option>
        ))}
      </select>
    </Field>
  );
}

export function CheckboxField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: boolean;
  onChange: (value: boolean) => void;
}) {
  return (
    <Field label={label}>
      <input
        type="checkbox"
        checked={value}
        onChange={(e) => onChange(e.target.checked)}
      />
    </Field>
  );
}
