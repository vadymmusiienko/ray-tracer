import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Ray Tracer — Scene Builder",
  description:
    "Build a scene with a form and render it with a C# Whitted-style ray tracer.",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
