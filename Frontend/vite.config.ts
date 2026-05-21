import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

const isGitHubPages = process.env.GITHUB_PAGES === "true";
const devApiTarget = process.env.VITE_DEV_API_BASE_URL ?? "https://localhost:7100";

export default defineConfig({
  base: isGitHubPages ? "/Logistics_project/" : "/",
  plugins: [react(), tailwindcss()],
  server: {
    host: "0.0.0.0",
    proxy: {
      "/api": {
        target: devApiTarget,
        changeOrigin: true,
        secure: false,
      },
      "/shipments": {
        target: devApiTarget,
        changeOrigin: true,
        secure: false,
      },
    },
    allowedHosts: [
      "ingrainedly-hyperdemocratic-joleen.ngrok-free.dev",
      "unmultipliable-kelsey-unloyal.ngrok-free.dev",
    ],
  },
});
