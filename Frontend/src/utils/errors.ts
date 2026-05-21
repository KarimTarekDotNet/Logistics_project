import { ApiError } from "../services/api";

export function getErrorMessage(error: unknown) {
  if (error instanceof ApiError) return error.message;
  if (error instanceof Error) return error.message;
  return "Unexpected error";
}

const guidPattern = /\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b/i;
const guidReplacePattern = /\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b/gi;

export function getFriendlyErrorMessage(error: unknown, fallback = "The request could not be completed. Please try again.") {
  const rawMessage = getErrorMessage(error);
  const message = rawMessage.replace(/\s+/g, " ").trim();
  const lower = message.toLowerCase();

  if (!message) return fallback;
  if (lower.includes("no quotes found for route")) return "No quotes found for this route.";
  if (lower.includes("no rates found")) return "No rates found for the selected criteria.";
  if (lower.includes("invoice not found")) return "No invoices found for this shipment.";
  if (lower.includes("request failed with status 404")) return "No records found for this selection.";
  if (lower.includes("non-empty request body") || lower.includes("request field is required")) {
    return "Select a route, container type, and supported currency before loading analytics.";
  }
  if (lower.includes("user not found") || lower.includes("customer profile not found")) {
    return "Your account is not allowed to complete this action yet.";
  }
  if (lower.includes("shipmentcharge.shipment.get") || lower.includes("nullreferenceexception") || lower.includes("system.")) {
    return "The server could not complete this request.";
  }
  if (lower.includes("stack trace") || lower.includes(" at ")) return fallback;
  if (guidPattern.test(message)) return message.replace(guidReplacePattern, "this record").replace(/\s+/g, " ").trim();

  return message;
}

export function logTechnicalError(context: string, error: unknown) {
  if (import.meta.env.DEV) {
    console.error(context, error);
  }
}

export function isNotFoundError(error: unknown) {
  return error instanceof ApiError && error.status === 404;
}

export async function safe<T>(call: () => Promise<T>, fallback: T) {
  try {
    return await call();
  } catch {
    return fallback;
  }
}
