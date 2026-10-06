import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  insecureSkipTLSVerify: true,
  vus: 50,
  duration: '30s',
  thresholds: {
    http_req_duration: ['p(95)<300'],
    http_req_failed: ['rate<0.01'],
  },
};

const BASE = 'https://localhost:7143/api/v1';

export default function () {
  const page = Math.floor(Math.random() * 10) + 1;

  const list = http.get(`${BASE}/policies?page=${page}&size=20&sort=premiumAmount,desc`);
  check(list, { 'list is 200': (r) => r.status === 200 });

  const summary = http.get(`${BASE}/policies/summary`);
  check(summary, { 'summary is 200': (r) => r.status === 200 });

  sleep(0.2);
}