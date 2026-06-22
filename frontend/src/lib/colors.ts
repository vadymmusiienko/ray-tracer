import type { Vec3 } from "./types";

// Linear [0,1] RGB triplet <-> "#rrggbb" for the native <input type="color">.

export function rgbToHex(rgb: Vec3): string {
  const channel = (x: number) =>
    Math.max(0, Math.min(255, Math.round((x || 0) * 255)))
      .toString(16)
      .padStart(2, "0");
  return `#${channel(rgb[0])}${channel(rgb[1])}${channel(rgb[2])}`;
}

export function hexToRgb(hex: string): Vec3 {
  const n = parseInt(hex.slice(1), 16);
  return [((n >> 16) & 255) / 255, ((n >> 8) & 255) / 255, (n & 255) / 255];
}
