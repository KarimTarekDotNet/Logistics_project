import type {
  AuthResponse,
  Carrier,
  ContainerType,
  Customer,
  Invoice,
  Port,
  ProfileResponse,
  ProfileUpdateResponse,
  QueryParams,
  Quote,
  QuoteRequest,
  Rate,
  Route,
  Shipment,
  ShipmentCharge,
  ShipmentDocument,
  TimelineItem
} from "./types";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";

type RequestOptions = {
  method?: string;
  body?: unknown;
  token?: string;
  headers?: Record<string, string>;
};

export class ApiError extends Error {
  status: number;
  payload: unknown;

  constructor(status: number, message: string, payload: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

function buildQuery(params?: QueryParams | Record<string, string | number | boolean | undefined>) {
  if (!params) return "";

  const query = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") return;
    query.set(key, String(value));
  });

  const result = query.toString();
  return result ? `?${result}` : "";
}

async function parseResponse(response: Response) {
  if (response.status === 204) return null;

  const contentType = response.headers.get("content-type") ?? "";
  if (contentType.includes("application/json")) {
    return response.json();
  }

  const text = await response.text();
  return text || null;
}

function parseJsonString(value: string) {
  try {
    return JSON.parse(value);
  } catch {
    return null;
  }
}

function cleanMessage(value: string) {
  return value
    .replace(/\s+/g, " ")
    .replace(/["{}[\]]/g, "")
    .trim()
    .slice(0, 240);
}

function extractErrorStrings(value: unknown): string[] {
  if (typeof value === "string") {
    const parsed = value.trim().startsWith("{") || value.trim().startsWith("[") ? parseJsonString(value) : null;
    return parsed ? extractErrorStrings(parsed) : [cleanMessage(value)];
  }

  if (Array.isArray(value)) {
    return value.flatMap(extractErrorStrings);
  }

  if (typeof value === "object" && value) {
    const record = value as Record<string, unknown>;
    const direct = [record.message, record.Message, record.error, record.Error, record.detail]
      .flatMap(extractErrorStrings)
      .filter(Boolean);

    if (direct.length > 0) return direct;

    if (record.errors) return extractErrorStrings(record.errors);

    if (record.title) return extractErrorStrings(record.title);

    return Object.values(record).flatMap(extractErrorStrings);
  }

  return [];
}

function extractApiMessage(payload: unknown, status: number) {
  const messages = extractErrorStrings(payload).filter((message) => message.length > 0);

  if (messages.length > 0) return Array.from(new Set(messages)).slice(0, 2).join(" ");

  return `Request failed with status ${status}`;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const isFormData = options.body instanceof FormData;
  const requestBody: BodyInit | undefined = isFormData
    ? (options.body as FormData)
    : options.body
      ? JSON.stringify(options.body)
      : undefined;
  const headers: Record<string, string> = {
    ...(isFormData ? {} : { "Content-Type": "application/json" }),
    ...(options.token ? { Authorization: `Bearer ${options.token}` } : {}),
    ...options.headers
  };

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: requestBody
  });

  const payload = await parseResponse(response);

  if (!response.ok) {
    throw new ApiError(response.status, extractApiMessage(payload, response.status), payload);
  }

  return payload as T;
}

export const api = {
  baseUrl: API_BASE_URL,

  login(body: { identity: string; password: string }) {
    return request<AuthResponse>("/api/Auth/login", { method: "POST", body });
  },

  register(body: {
    firstName: string;
    lastName: string;
    userName: string;
    email: string;
    countryCode: string;
    phoneNumber: string;
    password: string;
    confirmPassword: string;
  }) {
    return request<AuthResponse>("/api/Auth/register", { method: "POST", body });
  },

  confirmEmail(userId: string, token: string) {
    return request<AuthResponse>(
      `/api/Auth/confirm-email${buildQuery({ userId, token })}`
    );
  },

  resendEmailConfirmation(email: string) {
    return request<AuthResponse>(
      `/api/Auth/resend-email-confirmation${buildQuery({ Email: email })}`,
      { method: "POST" }
    );
  },

  confirmPhone(phone: string, code: string) {
    return request<AuthResponse>("/api/Auth/confirm-phone", {
      method: "POST",
      body: { phone, code }
    });
  },

  resendPhoneOtp(phone: string) {
    return request<AuthResponse>(
      `/api/Auth/resend-phone-otp${buildQuery({ phone })}`,
      { method: "POST" }
    );
  },

  logout(refreshToken: string, token: string) {
    return request<boolean>("/api/Auth/logout", {
      method: "POST",
      token,
      body: { refreshToken }
    });
  },

  getRates(token: string, params?: QueryParams) {
    return request<Rate[]>(`/api/rates${buildQuery(params)}`, { token });
  },

  getPublicRateCount() {
    return request<number>("/api/rates/count");
  },

  createRate(token: string, body: {
    carrierId: string;
    routeId: string;
    containerTypeId: string;
    price: number;
    currency: string;
    validFrom: string;
    validTo: string;
    maxGrossWeightKg?: number;
    maxNetWeightKg?: number;
    maxVolumeCbm?: number;
    allowsHazardous?: boolean;
    minTemperatureCelsius?: number;
    maxTemperatureCelsius?: number;
  }) {
    return request<Rate>("/api/rates", { method: "POST", token, body });
  },

  toggleRate(token: string, id: string) {
    return request<boolean>(`/api/rates/${id}/active`, { method: "PATCH", token });
  },

  getCarriers(params?: QueryParams) {
    return request<Carrier[]>(`/api/carriers${buildQuery(params)}`);
  },

  getPorts(params?: QueryParams) {
    return request<Port[]>(`/api/ports${buildQuery(params)}`);
  },

  getRoutes(params?: QueryParams) {
    return request<Route[]>(`/api/routes${buildQuery(params)}`);
  },

  getContainerTypes(params?: QueryParams) {
    return request<ContainerType[]>(`/api/container-types${buildQuery(params)}`);
  },

  getQuotes(token: string, params?: QueryParams) {
    return request<Quote[]>(`/api/quotes${buildQuery(params)}`, { token });
  },

  getMyQuotes(token: string, params?: QueryParams) {
    return request<Quote[]>(`/api/quotes/my${buildQuery(params)}`, { token });
  },

  createQuote(token: string, body: {
    customerId: string;
    rateId: string;
    requestedGrossWeightKg: number;
    requestedNetWeightKg: number;
    requestedVolumeCbm: number;
    isHazardous: boolean;
    requiredTemperatureCelsius?: number;
  }) {
    return request<Quote>("/api/quotes", { method: "POST", token, body });
  },

  getQuoteRequests(token: string, params?: QueryParams) {
    return request<QuoteRequest[]>(`/api/QuoteRequest${buildQuery(params)}`, { token });
  },

  getMyQuoteRequests(token: string, params?: QueryParams) {
    return request<QuoteRequest[]>(`/api/QuoteRequest/my${buildQuery(params)}`, { token });
  },

  getQuoteRequest(token: string, id: string) {
    return request<QuoteRequest>(`/api/QuoteRequest/${id}`, { token });
  },

  createQuoteRequestFromRate(token: string, body: {
    rateId: string;
    requestedGrossWeightKg: number;
    requestedNetWeightKg: number;
    requestedVolumeCbm: number;
    isHazardous: boolean;
    requiredTemperatureCelsius?: number;
    notes?: string;
  }) {
    return request<QuoteRequest>("/api/QuoteRequest/from-rate", { method: "POST", token, body });
  },

  approveQuoteRequest(token: string, id: string) {
    return request<QuoteRequest>(`/api/QuoteRequest/${id}/approve`, { method: "PATCH", token });
  },

  rejectQuoteRequest(token: string, id: string, reason: string) {
    return request<QuoteRequest>(`/api/QuoteRequest/${id}/reject`, {
      method: "PATCH",
      token,
      body: { reason }
    });
  },

  cancelQuoteRequest(token: string, id: string) {
    return request<QuoteRequest>(`/api/QuoteRequest/${id}/cancel`, { method: "PATCH", token });
  },

  getCustomers(token: string, params?: QueryParams) {
    return request<Customer[]>(`/api/Customer${buildQuery(params)}`, { token });
  },

  getMyCustomer(token: string) {
    return request<Customer>("/api/Customer/me", { token });
  },

  createCustomer(token: string, body: {
    nationalId?: string;
    dateOfBirth?: string;
    companyName?: string;
    taxNumber?: string;
    countryCode?: string;
  }) {
    return request<Customer>("/api/Customer", { method: "POST", token, body });
  },

  updateCustomer(token: string, body: {
    nationalId?: string;
    dateOfBirth?: string;
    companyName?: string;
    taxNumber?: string;
    countryCode?: string;
  }) {
    return request<Customer>("/api/Customer", { method: "PUT", token, body });
  },

  deleteCustomer(token: string) {
    return request<string>("/api/Customer", { method: "DELETE", token });
  },

  getProfile(token: string) {
    return request<ProfileResponse>("/api/user/profile", { token });
  },

  updateProfile(token: string, body: {
    firstName?: string;
    lastName?: string;
    username?: string;
    email?: string;
    phoneNumber?: string;
  }) {
    return request<ProfileUpdateResponse>("/api/user/profile", {
      method: "PUT",
      token,
      body
    });
  },

  confirmProfileEmailChange(userId: string, token: string) {
    return request<ProfileUpdateResponse>(
      `/api/user/profile/confirm-email-change${buildQuery({ userId, token })}`
    );
  },

  updatePassword(token: string, body: {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
  }) {
    return request<{ success: boolean; message: string }>("/api/user/profile/password", {
      method: "PUT",
      token,
      body
    });
  },

  verifyPhoneChange(token: string, code: string) {
    return request<ProfileUpdateResponse>("/api/user/profile/verify-phone-change", {
      method: "POST",
      token,
      body: { code }
    });
  },

  getShipments(token: string, params?: QueryParams) {
    return request<Shipment[]>(`/api/Shipment${buildQuery(params)}`, { token });
  },

  getPublicShipmentCount() {
    return request<number>("/api/Shipment/Count");
  },

  getMyShipments(token: string, params?: QueryParams) {
    return request<Shipment[]>(`/api/Shipment/my${buildQuery(params)}`, { token });
  },

  getShipment(token: string, id: string) {
    return request<Shipment>(`/api/Shipment/${id}`, { token });
  },

  createShipment(token: string, quoteId: string) {
    return request<Shipment>("/api/Shipment", {
      method: "POST",
      token,
      body: { quoteId }
    });
  },

  updateTracking(token: string, id: string, body: Record<string, string | undefined>) {
    return request<Shipment>(`/api/Shipment/${id}/tracking`, {
      method: "PUT",
      token,
      body
    });
  },

  shipmentAction(token: string, id: string, action: string, reason?: string) {
    return request<Shipment>(`/api/Shipment/${id}/${action}`, {
      method: "PATCH",
      token,
      body: { reason }
    });
  },

  getTimeline(token: string, id: string, params?: QueryParams) {
    return request<TimelineItem[]>(`/api/Shipment/${id}/timeline${buildQuery(params)}`, {
      token
    });
  },

  generateCharges(token: string, shipmentId: string, options: { chargeType?: number; payerType?: number } = {}) {
    return request<ShipmentCharge[]>("/api/ShipmentCharge/generate", {
      method: "POST",
      token,
      body: {
        shipmentId,
        chargeType: options.chargeType ?? 0,
        payerType: options.payerType ?? 0
      }
    });
  },

  getInvoicesByShipment(token: string, shipmentId: string) {
    return request<Invoice[]>(`/api/Invoice/shipment/${shipmentId}`, { token });
  },

  createInvoice(token: string, shipmentId: string) {
    return request<Invoice>(`/api/Invoice/${shipmentId}`, { method: "POST", token });
  },

  invoiceStatus(token: string, id: string, action: "mark-as-paid" | "mark-as-partially-paid" | "mark-as-refunded", price?: number) {
    return request<Invoice>(`/api/Invoice/${id}/${action}`, {
      method: "PATCH",
      token,
      body: action === "mark-as-partially-paid" && price !== undefined ? { Price: price } : undefined
    });
  },

  confirmInvoice(token: string, id: string) {
    return request<Invoice>(`/api/Invoice/${id}/confirm`, {
      method: "PATCH",
      token
    });
  },

  cancelInvoice(token: string, id: string, reason: string) {
    return request<Invoice>(`/api/Invoice/${id}/cancel`, {
      method: "PATCH",
      token,
      body: { reason }
    });
  },

  getDocuments(token: string, shipmentId: string) {
    return request<ShipmentDocument[]>(`/api/ShipmentDocument/shipment/${shipmentId}`, { token });
  },

  uploadDocument(token: string, shipmentId: string, formData: FormData) {
    return request<ShipmentDocument>(`/api/ShipmentDocument/shipment/${shipmentId}`, {
      method: "POST",
      token,
      body: formData
    });
  },

};
