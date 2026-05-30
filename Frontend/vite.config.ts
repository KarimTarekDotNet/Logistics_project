import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import type { IncomingMessage } from "node:http";

const isGitHubPages = process.env.GITHUB_PAGES === "true";
const devApiTarget = process.env.VITE_DEV_API_BASE_URL ?? "https://localhost:7100";
const csrfCookieName = "XSRF-TOKEN";
const csrfRequestHeaderName = "X-CSRF-REQUEST-TOKEN";

function readCookieValue(cookie: string) {
  return cookie
    .slice(cookie.indexOf("=") + 1)
    .replace(/;.*$/, "");
}

function normalizeDevCookieAttributes(cookie: string) {
  return cookie
    .replace(/;\s*secure/gi, "")
    .replace(/;\s*samesite=none/gi, "; samesite=lax");
}

function normalizeDevCsrfCookies(proxyRes: IncomingMessage) {
  const url = proxyRes.req?.path?.toLowerCase() ?? "";

  const rawSetCookie = proxyRes.headers["set-cookie"];
  const setCookies = Array.isArray(rawSetCookie) ? rawSetCookie : rawSetCookie ? [rawSetCookie] : [];
  if (setCookies.length > 0) {
    proxyRes.headers["set-cookie"] = setCookies.map(normalizeDevCookieAttributes);
  }

  if (!url.startsWith("/api/auth/csrf-token")) return;

  const xsrfCookies = setCookies.filter((cookie) => cookie.startsWith(`${csrfCookieName}=`));

  if (xsrfCookies.length < 2) return;

  const [cookieToken, requestToken] = xsrfCookies;
  const requestTokenValue = readCookieValue(requestToken);
  const passthroughCookies = setCookies.filter((cookie) => !cookie.startsWith(`${csrfCookieName}=`));

  proxyRes.headers["set-cookie"] = [cookieToken, ...passthroughCookies].map(normalizeDevCookieAttributes);
  proxyRes.headers[csrfRequestHeaderName.toLowerCase()] = requestTokenValue;
  proxyRes.headers["access-control-expose-headers"] = [
    proxyRes.headers["access-control-expose-headers"],
    csrfRequestHeaderName
  ]
    .filter(Boolean)
    .join(", ");
}

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
        configure: (proxy) => {
          proxy.on("proxyRes", normalizeDevCsrfCookies);
        },
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
