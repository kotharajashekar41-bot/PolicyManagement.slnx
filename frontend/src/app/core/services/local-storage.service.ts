import { Injectable } from '@angular/core';

/** Single seam between the app and `window.localStorage`. Nothing else in the app
 * touches localStorage directly — that keeps storage failures (private browsing,
 * disabled storage, quota) contained to one place, and means swapping the backing
 * store (e.g. to a server-synced preference) only touches this file. */
@Injectable({ providedIn: 'root' })
export class LocalStorageService {
  get<T>(key: string): T | null {
    try {
      const raw = localStorage.getItem(key);
      return raw === null ? null : (JSON.parse(raw) as T);
    } catch {
      // Storage inaccessible (private mode, disabled, or a corrupt value) — treat as unset.
      return null;
    }
  }

  set<T>(key: string, value: T): void {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch {
      // Best-effort: a failed write just means the preference won't persist this session.
    }
  }

  remove(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch {
      // No-op — nothing to clean up if storage isn't available.
    }
  }
}
