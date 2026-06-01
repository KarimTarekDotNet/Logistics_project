import { SHIPMENT_WORKFLOW_RESUME_KEY } from "../constants/logistics";

export type ShipmentWorkflowStep = "charges" | "invoice";

export type ShipmentWorkflowResume = {
  userId?: string;
  shipmentId: string;
  step: ShipmentWorkflowStep;
  invoiceId?: string;
  updatedAt: string;
};

function isWorkflowStep(value: unknown): value is ShipmentWorkflowStep {
  return value === "charges" || value === "invoice";
}

function readStoredResume() {
  try {
    return JSON.parse(localStorage.getItem(SHIPMENT_WORKFLOW_RESUME_KEY) ?? "null") as Partial<ShipmentWorkflowResume> | null;
  } catch {
    return null;
  }
}

export function loadShipmentWorkflowResume(userId?: string) {
  const stored = readStoredResume();
  if (!stored?.shipmentId || !isWorkflowStep(stored.step)) return null;
  if (stored.userId && userId && stored.userId !== userId) return null;

  return {
    userId: stored.userId,
    shipmentId: stored.shipmentId,
    step: stored.step,
    invoiceId: stored.invoiceId,
    updatedAt: stored.updatedAt || new Date().toISOString()
  } satisfies ShipmentWorkflowResume;
}

export function saveShipmentWorkflowResume(resume: Omit<ShipmentWorkflowResume, "updatedAt">) {
  localStorage.setItem(
    SHIPMENT_WORKFLOW_RESUME_KEY,
    JSON.stringify({
      ...resume,
      updatedAt: new Date().toISOString()
    })
  );
}

export function clearShipmentWorkflowResume(userId?: string, shipmentId?: string) {
  const stored = loadShipmentWorkflowResume(userId);
  if (!stored) return;
  if (shipmentId && stored.shipmentId !== shipmentId) return;
  localStorage.removeItem(SHIPMENT_WORKFLOW_RESUME_KEY);
}
