const base = process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000";

export async function api<T>(path: string, init?: RequestInit): Promise<T> {
  const url = `${base}${path}`;
  const res = await fetch(url, {
    headers: { "Content-Type": "application/json", ...(init?.headers || {}) },
    ...init
  });
  if (!res.ok) {
    const ct = res.headers.get("content-type") || "";
    const body = ct.includes("application/problem+json") ? await res.json() : await res.text();
    throw new Error(typeof body === "string" ? body : body?.title || `HTTP ${res.status}`);
  }
  const ct = res.headers.get("content-type") || "";
  if (ct.includes("application/json")) return (await res.json()) as T;
  return (await res.text()) as unknown as T;
}

