"use client";

import { create } from "zustand";
import { api } from "@/lib/api";

export type Form = {
  id: string;
  tenantId: string;
  name: string;
  description?: string | null;
  createdAtUtc: string;
};

export type PagedResult<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};

type FormsState = {
  items: Form[];
  total: number;
  loading: boolean;
  error: string | null;
  fetch: (tenantId: string, page?: number, pageSize?: number) => Promise<void>;
  create: (tenantId: string, name: string, description?: string | null) => Promise<string>;
  update: (id: string, tenantId: string, name: string, description?: string | null) => Promise<void>;
  remove: (id: string, tenantId: string) => Promise<void>;
  clearError: () => void;
};

export const useFormsStore = create<FormsState>((set, get) => ({
  items: [],
  total: 0,
  loading: false,
  error: null,
  clearError: () => set({ error: null }),
  fetch: async (tenantId, page = 1, pageSize = 20) => {
    set({ loading: true, error: null });
    try {
      const data = await api<PagedResult<Form>>(
        `/api/v1/forms?tenantId=${encodeURIComponent(tenantId)}&page=${page}&pageSize=${pageSize}`
      );
      set({ items: data.items, total: data.total, loading: false });
    } catch (e: any) {
      set({ error: e?.message ?? "Failed to load forms", loading: false });
    }
  },
  create: async (tenantId, name, description) => {
    set({ error: null });
    const body = { tenantId, name, description: description ?? null };
    const res = await api<{ id: string }>(`/api/v1/forms`, {
      method: "POST",
      body: JSON.stringify(body)
    });
    // optimistic refresh
    const { fetch } = get();
    fetch(tenantId).catch(() => {});
    return res.id;
  },
  update: async (id, tenantId, name, description) => {
    set({ error: null });
    const body = { tenantId, name, description: description ?? null };
    await api(`/api/v1/forms/${id}`, { method: "PUT", body: JSON.stringify(body) });
    const items = get().items.map((f) => (f.id === id ? { ...f, name, description: description ?? null } : f));
    set({ items });
  },
  remove: async (id, tenantId) => {
    set({ error: null });
    await api(`/api/v1/forms/${id}?tenantId=${encodeURIComponent(tenantId)}`, { method: "DELETE" });
    set({ items: get().items.filter((f) => f.id !== id), total: Math.max(0, get().total - 1) });
  }
}));

