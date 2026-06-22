"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import {
  fetchCapabilities,
  fetchExample,
  fetchExampleIndex,
  RenderError,
  renderScene,
} from "@/lib/api";
import {
  emptyScene,
  FALLBACK_CAPABILITIES,
  newLight,
  newMaterial,
  newObject,
} from "@/lib/defaults";
import { fromDto, toDto } from "@/lib/scene";
import type {
  Capabilities,
  ExampleMeta,
  LightForm,
  MaterialForm,
  ObjectForm,
  SceneForm,
} from "@/lib/types";
import Preview, { type PreviewState } from "./Preview";
import Section from "./Section";
import CameraPanel from "./panels/CameraPanel";
import LightsPanel from "./panels/LightsPanel";
import MaterialsPanel from "./panels/MaterialsPanel";
import ObjectsPanel from "./panels/ObjectsPanel";
import RenderSettingsPanel from "./panels/RenderSettingsPanel";
import styles from "./SceneBuilder.module.css";

export default function SceneBuilder() {
  const [caps, setCaps] = useState<Capabilities>(FALLBACK_CAPABILITIES);
  const [scene, setScene] = useState<SceneForm>(emptyScene);
  const [examples, setExamples] = useState<ExampleMeta[]>([]);
  const [selectedExample, setSelectedExample] = useState("");
  const [preview, setPreview] = useState<PreviewState>({
    imageUrl: null,
    status: "",
    busy: false,
    errors: [],
  });

  // Keep the latest caps available to async callbacks without re-creating them.
  const capsRef = useRef(caps);
  capsRef.current = caps;
  const objectUrlRef = useRef<string | null>(null);

  // --- one-time bootstrap: capabilities + examples, then load the first ---
  useEffect(() => {
    let cancelled = false;

    (async () => {
      let loadedCaps = FALLBACK_CAPABILITIES;
      try {
        loadedCaps = await fetchCapabilities();
      } catch {
        /* fall back to defaults; the API may not be running yet */
      }
      if (cancelled) return;
      setCaps(loadedCaps);

      let list: ExampleMeta[] = [];
      try {
        list = await fetchExampleIndex();
      } catch {
        /* no examples; start with an empty scene */
      }
      if (cancelled) return;
      setExamples(list);

      if (list.length > 0) {
        setSelectedExample(list[0].id);
        try {
          const dto = await fetchExample(list[0].file);
          if (!cancelled) setScene(fromDto(dto, loadedCaps));
        } catch {
          /* keep the empty scene */
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  // Revoke the last blob URL when the component unmounts.
  useEffect(() => {
    return () => {
      if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
    };
  }, []);

  // --- scene mutation helpers (all immutable) ---
  const patchRender = useCallback((patch: Partial<SceneForm["render"]>) => {
    setScene((s) => ({ ...s, render: { ...s.render, ...patch } }));
  }, []);

  const patchCamera = useCallback((patch: Partial<SceneForm["camera"]>) => {
    setScene((s) => ({ ...s, camera: { ...s.camera, ...patch } }));
  }, []);

  const patchMaterial = useCallback(
    (key: string, patch: Partial<MaterialForm>) => {
      setScene((s) => ({
        ...s,
        materials: s.materials.map((m) =>
          m.key === key ? { ...m, ...patch } : m,
        ),
      }));
    },
    [],
  );

  const patchLight = useCallback((key: string, patch: Partial<LightForm>) => {
    setScene((s) => ({
      ...s,
      lights: s.lights.map((l) => (l.key === key ? { ...l, ...patch } : l)),
    }));
  }, []);

  const patchObject = useCallback((key: string, patch: Partial<ObjectForm>) => {
    setScene((s) => ({
      ...s,
      objects: s.objects.map((o) => (o.key === key ? { ...o, ...patch } : o)),
    }));
  }, []);

  // A material id changed: refresh the form so object dropdowns re-read ids.
  // (State already updated; this is just to keep the mental model explicit.)
  const commitMaterialIds = useCallback(() => {
    setScene((s) => ({ ...s }));
  }, []);

  const loadSelectedExample = useCallback(async () => {
    const meta = examples.find((e) => e.id === selectedExample);
    if (!meta) return;
    setPreview((p) => ({
      ...p,
      status: `Loading “${meta.name}”…`,
      errors: [],
    }));
    try {
      const dto = await fetchExample(meta.file);
      setScene(fromDto(dto, capsRef.current));
      setPreview((p) => ({
        ...p,
        status: `Loaded “${meta.name}”. Hit Render.`,
      }));
    } catch (err) {
      setPreview((p) => ({ ...p, status: `Could not load example: ${err}` }));
    }
  }, [examples, selectedExample]);

  const doRender = useCallback(async () => {
    setPreview((p) => ({ ...p, busy: true, errors: [], status: "Rendering…" }));
    const startedAt = performance.now();
    try {
      const blob = await renderScene(toDto(scene));
      if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
      const url = URL.createObjectURL(blob);
      objectUrlRef.current = url;
      const ms = Math.round(performance.now() - startedAt);
      setPreview({
        imageUrl: url,
        busy: false,
        errors: [],
        status: `Rendered ${scene.render.width}×${scene.render.height} in ${ms} ms`,
      });
    } catch (err) {
      const messages =
        err instanceof RenderError ? err.messages : [String(err)];
      setPreview((p) => ({
        ...p,
        busy: false,
        errors: messages,
        status: "Render failed.",
      }));
    }
  }, [scene]);

  return (
    <div className={styles.app}>
      <header className={styles.header}>
        <h1 className={styles.title}>
          Ray Tracer <span className={styles.muted}>Scene Builder</span>
        </h1>
        <div className={styles.actions}>
          <select
            className={styles.select}
            value={selectedExample}
            title="Example scenes"
            onChange={(e) => setSelectedExample(e.target.value)}
          >
            {examples.length === 0 && <option value="">(no examples)</option>}
            {examples.map((ex) => (
              <option key={ex.id} value={ex.id} title={ex.description}>
                {ex.name}
              </option>
            ))}
          </select>
          <button
            className={styles.ghost}
            onClick={loadSelectedExample}
            disabled={examples.length === 0}
          >
            Load
          </button>
          <button
            className={styles.primary}
            onClick={doRender}
            disabled={preview.busy}
          >
            {preview.busy ? "Rendering…" : "Render"}
          </button>
        </div>
      </header>

      <main className={styles.main}>
        <section className={styles.builder}>
          <Section title="Render settings">
            <RenderSettingsPanel
              value={scene.render}
              limits={caps.limits}
              onChange={patchRender}
            />
          </Section>

          <Section title="Camera">
            <CameraPanel value={scene.camera} onChange={patchCamera} />
          </Section>

          <Section title="Materials" count={scene.materials.length}>
            <MaterialsPanel
              materials={scene.materials}
              caps={caps}
              onPatch={patchMaterial}
              onAdd={() =>
                setScene((s) => ({
                  ...s,
                  materials: [...s.materials, newMaterial(capsRef.current)],
                }))
              }
              onRemove={(key) =>
                setScene((s) => ({
                  ...s,
                  materials: s.materials.filter((m) => m.key !== key),
                }))
              }
              onIdCommit={commitMaterialIds}
            />
          </Section>

          <Section title="Lights" count={scene.lights.length}>
            <LightsPanel
              lights={scene.lights}
              onPatch={patchLight}
              onAdd={() =>
                setScene((s) => ({ ...s, lights: [...s.lights, newLight()] }))
              }
              onRemove={(key) =>
                setScene((s) => ({
                  ...s,
                  lights: s.lights.filter((l) => l.key !== key),
                }))
              }
            />
          </Section>

          <Section title="Objects" count={scene.objects.length}>
            <ObjectsPanel
              objects={scene.objects}
              materials={scene.materials}
              caps={caps}
              onPatch={patchObject}
              onAdd={() =>
                setScene((s) => ({
                  ...s,
                  objects: [...s.objects, newObject(s, capsRef.current)],
                }))
              }
              onRemove={(key) =>
                setScene((s) => ({
                  ...s,
                  objects: s.objects.filter((o) => o.key !== key),
                }))
              }
            />
          </Section>
        </section>

        <Preview state={preview} />
      </main>
    </div>
  );
}
