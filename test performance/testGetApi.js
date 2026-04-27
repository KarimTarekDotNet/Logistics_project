import http from 'k6/http';
import { check, sleep } from 'k6';

// =========================
// Config
// =========================
const BASE_URL = 'https://localhost:7100';

// لو عندك Authorize على الـ GET / PUT حط التوكن هنا
const BEARER_TOKEN = 'PUT_YOUR_BEARER_TOKEN_HERE';

// مفتاح الانتيجريشن
const INTEGRATION_API_KEY = 'PUT_YOUR_INTEGRATION_KEY_HERE';

// Seed data IDs
const RATE_ID = '00000000-0000-0000-0000-000000000042';
const CARRIER_ID = '00000000-0000-0000-0000-000000000011';
const ROUTE_ID = '00000000-0000-0000-0000-000000000031';
const CONTAINER_TYPE_ID = '00000000-0000-0000-0000-000000000020';

// =========================
// Options
// =========================
export const options = {
  scenarios: {
    // الحمل الحقيقي عندك غالبًا هنا
    get_rates: {
      executor: 'ramping-vus',
      exec: 'getRates',
      stages: [
        { duration: '15s', target: 25 },
        { duration: '20s', target: 50 },
        { duration: '20s', target: 100 },
        { duration: '15s', target: 150 },
        { duration: '10s', target: 0 },
      ],
      gracefulRampDown: '10s',
    },

    // غالبًا بيتستخدم أقل من list
    get_rate_by_id: {
      executor: 'constant-vus',
      exec: 'getRateById',
      vus: 15,
      duration: '45s',
      startTime: '5s',
    },

    // داخلي، فخليه قليل
    update_rate: {
      executor: 'constant-vus',
      exec: 'updateRate',
      vus: 2,
      duration: '20s',
      startTime: '10s',
    },

    // داخلي وأتقل، فخليه قليل جدًا
    integration_import: {
      executor: 'constant-vus',
      exec: 'importRateIntegration',
      vus: 1,
      duration: '15s',
      startTime: '15s',
    },
  },

  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1000'],
  },
};

// =========================
// Helpers
// =========================
function authHeaders() {
  return {
    headers: {
      Authorization: `Bearer ${BEARER_TOKEN}`,
      'Content-Type': 'application/json',
    },
  };
}

function integrationHeaders() {
  return {
    headers: {
      'X-Integration-Key': INTEGRATION_API_KEY,
      'Content-Type': 'application/json',
    },
  };
}

// لو بعض endpoints مش محتاجة auth
function jsonHeadersOnly() {
  return {
    headers: {
      'Content-Type': 'application/json',
    },
  };
}

// =========================
// 1) GET /api/rates
// =========================
export function getRates() {
  const url = `${BASE_URL}/api/rates?pageNumber=1&pageSize=10`;

  const res = http.get(url, authHeaders());

  check(res, {
    'GET rates status is 200': (r) => r.status === 200,
  });

  sleep(1);
}

// =========================
// 2) GET /api/rates/{id}
// =========================
export function getRateById() {
  const url = `${BASE_URL}/api/rates/${RATE_ID}`;

  const res = http.get(url, authHeaders());

  check(res, {
    'GET rate by id status is 200': (r) => r.status === 200,
  });

  sleep(1);
}

// =========================
// 3) PUT /api/rates/{id}
// WARNING: ده بيعدل الداتا فعليًا
// =========================
export function updateRate() {
  const payload = JSON.stringify({
    carrierId: CARRIER_ID,
    routeId: ROUTE_ID,
    containerTypeId: CONTAINER_TYPE_ID,
    price: 900 + __ITER, // تغيير بسيط كل مرة
    currency: 'USD',
    validFrom: '2026-01-01T00:00:00Z',
    validTo: '2026-12-31T00:00:00Z',
  });

  const url = `${BASE_URL}/api/rates/${RATE_ID}`;
  const res = http.put(url, payload, authHeaders());

  check(res, {
    'PUT rate status is success': (r) =>
      r.status === 200 || r.status === 204,
  });

  sleep(1);
}

// =========================
// 4) POST /api/integrations/rates/import
// WARNING: ده ممكن يعمل create أو update فعليًا
// =========================
export function importRateIntegration() {
  const uniqueExternalMessageId = `k6-${__VU}-${__ITER}-${Date.now()}`;

  const payload = JSON.stringify({
    source: 'email-carrier-test',
    rates: [
      {
        externalMessageId: uniqueExternalMessageId,
        carrierName: 'Mediterranean Shipping Company',
        fromPortCode: 'CNSHA',
        toPortCode: 'AEJEA',
        containerTypeName: '20ft Standard',
        price: 950 + __ITER,
        currency: 'USD',
        validFrom: '2026-01-01T00:00:00Z',
        validTo: '2026-12-31T00:00:00Z',
        rawSubject: 'New rate offer from carrier - k6 test',
      },
    ],
  });

  const url = `${BASE_URL}/api/integrations/rates/import`;
  const res = http.post(url, payload, integrationHeaders());

  check(res, {
    'Integration import status is success': (r) =>
      r.status === 200 || r.status === 201,
  });

  sleep(1);
}