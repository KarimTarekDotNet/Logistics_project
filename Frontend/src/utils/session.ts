import { PENDING_VERIFICATION_KEY, SESSION_KEY } from "../constants/logistics";
import type { AuthResponse, AuthSession } from "../types";

function decodeJwt(token: string) {
  try {
    const [, payload] = token.split(".");
    const normalized = payload.replace(/-/g, "+").replace(/_/g, "/");
    return JSON.parse(atob(normalized)) as Record<string, unknown>;
  } catch {
    return {};
  }
}

function normalizeRoles(raw: unknown) {
  if (Array.isArray(raw)) return raw.map(String);
  if (typeof raw === "string") return [raw];
  return [];
}

export function sessionFromAuth(response: AuthResponse): AuthSession {
  const token = response.accessToken ?? "";
  const claims = decodeJwt(token);
  const roleClaim =
    claims.role ??
    claims.roles ??
    claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

  return {
    accessToken: token,
    refreshToken: response.refreshToken,
    id:
      response.id ??
      String(
        claims.nameid ??
          claims.sub ??
          claims["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ??
          ""
      ),
    userName: response.userName ?? String(claims.unique_name ?? claims.name ?? ""),
    email: response.email ?? String(claims.email ?? ""),
    roles: normalizeRoles(roleClaim),
    expiresAt: response.expiration
  };
}

export function loadStoredSession() {
  const stored = localStorage.getItem(SESSION_KEY);
  if (!stored) return null;

  try {
    return JSON.parse(stored) as AuthSession;
  } catch {
    localStorage.removeItem(SESSION_KEY);
    return null;
  }
}

export function persistSession(session: AuthSession | null) {
  if (!session) {
    localStorage.removeItem(SESSION_KEY);
    return;
  }

  localStorage.setItem(SESSION_KEY, JSON.stringify(session));
}

export function loadPendingVerification() {
  const stored = localStorage.getItem(PENDING_VERIFICATION_KEY);
  if (!stored) return { email: "", phone: "" };

  try {
    const parsed = JSON.parse(stored) as { email?: string; phone?: string };
    return { email: parsed.email ?? "", phone: parsed.phone ?? "" };
  } catch {
    localStorage.removeItem(PENDING_VERIFICATION_KEY);
    return { email: "", phone: "" };
  }
}

export function maskPhone(phone: string) {
  const compact = phone.replace(/\s+/g, "");
  if (!compact) return "your registered phone";
  const lastFour = compact.slice(-4);
  const prefix = compact.startsWith("+") ? compact.slice(0, 3) : compact.slice(0, 2);
  return `${prefix}*****${lastFour}`;
}
