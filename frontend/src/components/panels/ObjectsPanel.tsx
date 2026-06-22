"use client";

import {
  NumberField,
  SelectField,
  Vec3Field,
} from "@/components/fields/NumberField";
import type {
  Capabilities,
  MaterialForm,
  ObjectForm,
  ObjectType,
} from "@/lib/types";
import EditorCard from "./EditorCard";
import styles from "./panels.module.css";

function ObjectCard({
  object,
  caps,
  materialIds,
  onPatch,
  onRemove,
}: {
  object: ObjectForm;
  caps: Capabilities;
  materialIds: string[];
  onPatch: (patch: Partial<ObjectForm>) => void;
  onRemove: () => void;
}) {
  const materialOptions = materialIds.length
    ? materialIds
    : ["(define a material)"];

  return (
    <EditorCard tag="Object" onRemove={onRemove}>
      <SelectField
        label="Type"
        options={caps.objectTypes}
        value={object.type}
        onChange={(type) => onPatch({ type: type as ObjectType })}
      />
      <SelectField
        label="Material"
        options={materialOptions}
        value={object.material}
        onChange={(material) => onPatch({ material })}
      />

      {object.type === "sphere" && (
        <>
          <Vec3Field
            label="Center (x, y, z)"
            value={object.center}
            onChange={(center) => onPatch({ center })}
          />
          <NumberField
            label="Radius"
            value={object.radius}
            onChange={(radius) => onPatch({ radius })}
          />
        </>
      )}

      {object.type === "plane" && (
        <>
          <Vec3Field
            label="Point (x, y, z)"
            value={object.point}
            onChange={(point) => onPatch({ point })}
          />
          <Vec3Field
            label="Normal (x, y, z)"
            value={object.normal}
            onChange={(normal) => onPatch({ normal })}
          />
        </>
      )}

      {object.type === "triangle" && (
        <>
          <Vec3Field
            label="Vertex 0"
            value={object.v0}
            onChange={(v0) => onPatch({ v0 })}
          />
          <Vec3Field
            label="Vertex 1"
            value={object.v1}
            onChange={(v1) => onPatch({ v1 })}
          />
          <Vec3Field
            label="Vertex 2"
            value={object.v2}
            onChange={(v2) => onPatch({ v2 })}
          />
        </>
      )}

      {object.type === "objModel" && (
        <>
          <SelectField
            label="Model"
            options={caps.models.length ? caps.models : ["(no models)"]}
            value={object.model}
            onChange={(model) => onPatch({ model })}
          />
          <Vec3Field
            label="Position (x, y, z)"
            value={object.position}
            onChange={(position) => onPatch({ position })}
          />
          <Vec3Field
            label="Rotation axis (x, y, z)"
            value={object.rotationAxis}
            onChange={(rotationAxis) => onPatch({ rotationAxis })}
          />
          <NumberField
            label="Rotation angle (°)"
            value={object.rotationAngle}
            onChange={(rotationAngle) => onPatch({ rotationAngle })}
          />
          <NumberField
            label="Scale"
            value={object.scale}
            onChange={(scale) => onPatch({ scale })}
          />
        </>
      )}
    </EditorCard>
  );
}

export default function ObjectsPanel({
  objects,
  materials,
  caps,
  onPatch,
  onAdd,
  onRemove,
}: {
  objects: ObjectForm[];
  materials: MaterialForm[];
  caps: Capabilities;
  onPatch: (key: string, patch: Partial<ObjectForm>) => void;
  onAdd: () => void;
  onRemove: (key: string) => void;
}) {
  const materialIds = materials.map((m) => m.id);
  return (
    <>
      <div className={styles.list}>
        {objects.map((o) => (
          <ObjectCard
            key={o.key}
            object={o}
            caps={caps}
            materialIds={materialIds}
            onPatch={(patch) => onPatch(o.key, patch)}
            onRemove={() => onRemove(o.key)}
          />
        ))}
      </div>
      <button className={styles.add} onClick={onAdd}>
        + Add object
      </button>
    </>
  );
}
