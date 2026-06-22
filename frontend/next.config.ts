import type { NextConfig } from "next";

// The C# render API runs as a separate process (default http://localhost:5099).
// We proxy /api/* to it so the browser always talks to the Next origin — no CORS,
// and the same code works whether the backend is local or deployed elsewhere.
const API_ORIGIN = process.env.RAYTRACER_API_ORIGIN ?? "http://localhost:5099";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${API_ORIGIN}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
