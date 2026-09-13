const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '/api';

let accessToken: string | null = null;
let refreshInFlight: Promise<boolean> | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
}

export function clearAccessToken() {
  accessToken = null;
}

export function csrfToken() {
  return document.cookie.split('; ').find((item) => item.startsWith('XSRF-TOKEN='))?.split('=')[1] ?? '';
}

function errorMessage(body: string, status: number) {
  try {
    const parsed = JSON.parse(body) as { message?: string; detail?: string; title?: string };
    return parsed.message ?? parsed.detail ?? parsed.title ?? `Error HTTP ${status}`;
  } catch {
    return body || `Error HTTP ${status}`;
  }
}

async function execute(path: string, init: RequestInit = {}) {
  const headers = new Headers(init.headers);
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);
  return fetch(`${API_BASE_URL}${path}`, { ...init, headers, credentials: 'include' });
}

async function renovarAccessToken() {
  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      const response = await fetch(`${API_BASE_URL}/auth/renovar`, {
        method: 'POST',
        headers: { 'X-CSRF-TOKEN': csrfToken() },
        credentials: 'include',
      });
      if (!response.ok) {
        clearAccessToken();
        return false;
      }
      const session = await response.json() as { accessToken: string };
      setAccessToken(session.accessToken);
      return true;
    })().finally(() => { refreshInFlight = null; });
  }
  return refreshInFlight;
}

export async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  let response = await execute(path, init);
  if (response.status === 401 && path !== '/auth/renovar' && await renovarAccessToken()) response = await execute(path, init);
  return response;
}

export async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const response = await apiFetch(path, init);
  if (!response.ok) throw new Error(errorMessage(await response.text(), response.status));
  return response.status === 204 ? undefined as T : await response.json() as T;
}

export async function publicRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers);
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
  const response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers, credentials: 'include' });
  if (!response.ok) throw new Error(errorMessage(await response.text(), response.status));
  return response.status === 204 ? undefined as T : await response.json() as T;
}
