"use client";

import {
  ColorField,
  NumberField,
  SelectField,
  TextField,
} from "@/components/fields/NumberField";
import type { Capabilities, MaterialForm, MaterialType } from "@/lib/types";
import EditorCard from "./EditorCard";
import styles from "./panels.module.css";

const NONE = "(none)";

function MaterialCard({
  material,
  caps,
  onPatch,
  onRemove,
  onIdCommit,
}: {
  material: MaterialForm;
  caps: Capabilities;
  onPatch: (patch: Partial<MaterialForm>) => void;
  onRemove: () => void;
  onIdCommit: () => void;
}) {
  return (
    <EditorCard tag="Material" onRemove={onRemove}>
      <TextField
        label="ID"
        value={material.id}
        onChange={(id) => onPatch({ id })}
        onCommit={onIdCommit}
      />
      <SelectField
        label="Type"
        options={caps.materialTypes}
        value={material.type}
        onChange={(type) => onPatch({ type: type as MaterialType })}
      />

      {material.type === "standard" && (
        <>
          <ColorField
            label="Ambient"
            value={material.ambient}
            onChange={(ambient) => onPatch({ ambient })}
          />
          <ColorField
            label="Diffuse"
            value={material.diffuse}
            onChange={(diffuse) => onPatch({ diffuse })}
          />
          <ColorField
            label="Specular"
            value={material.specular}
            onChange={(specular) => onPatch({ specular })}
          />
          <NumberField
            label="Shininess"
            value={material.shininess}
            onChange={(shininess) => onPatch({ shininess })}
          />
          <NumberField
            label="Reflectivity"
            value={material.reflectivity}
            onChange={(reflectivity) => onPatch({ reflectivity })}
          />
          <NumberField
            label="Transmissivity"
            value={material.transmissivity}
            onChange={(transmissivity) => onPatch({ transmissivity })}
          />
          <NumberField
            label="Refractive index"
            value={material.refractiveIndex}
            onChange={(refractiveIndex) => onPatch({ refractiveIndex })}
          />
        </>
      )}

      {material.type === "texture" && (
        <>
          <SelectField
            label="Color map"
            options={caps.textures.length ? caps.textures : [NONE]}
            value={material.colorMap || NONE}
            onChange={(v) => onPatch({ colorMap: v === NONE ? "" : v })}
          />
          <SelectField
            label="Normal map"
            options={[NONE, ...caps.textures]}
            value={material.normalMap || NONE}
            onChange={(v) => onPatch({ normalMap: v === NONE ? "" : v })}
          />
        </>
      )}

      {material.type === "procedural" && (
        <>
          <SelectField
            label="Pattern"
            options={caps.patterns}
            value={material.pattern}
            onChange={(pattern) => onPatch({ pattern })}
          />
          <NumberField
            label="Scale U"
            value={material.scaleU}
            onChange={(scaleU) => onPatch({ scaleU })}
          />
          <NumberField
            label="Scale V"
            value={material.scaleV}
            onChange={(scaleV) => onPatch({ scaleV })}
          />
          <ColorField
            label="Color 1"
            value={material.color1}
            onChange={(color1) => onPatch({ color1 })}
          />
          <ColorField
            label="Color 2"
            value={material.color2}
            onChange={(color2) => onPatch({ color2 })}
          />
        </>
      )}
    </EditorCard>
  );
}

export default function MaterialsPanel({
  materials,
  caps,
  onPatch,
  onAdd,
  onRemove,
  onIdCommit,
}: {
  materials: MaterialForm[];
  caps: Capabilities;
  onPatch: (key: string, patch: Partial<MaterialForm>) => void;
  onAdd: () => void;
  onRemove: (key: string) => void;
  onIdCommit: () => void;
}) {
  return (
    <>
      <div className={styles.list}>
        {materials.map((m) => (
          <MaterialCard
            key={m.key}
            material={m}
            caps={caps}
            onPatch={(patch) => onPatch(m.key, patch)}
            onRemove={() => onRemove(m.key)}
            onIdCommit={onIdCommit}
          />
        ))}
      </div>
      <button className={styles.add} onClick={onAdd}>
        + Add material
      </button>
    </>
  );
}
