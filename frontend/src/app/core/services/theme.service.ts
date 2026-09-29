import { Injectable, effect, inject, signal } from '@angular/core';
import { LocalStorageService } from './local-storage.service';

export type ThemePreference = 'light' | 'dark' | 'system';

const STORAGE_KEY = 'policyplatform.theme';

/** Owns the light/dark/system preference and reflects it onto <html data-theme="...">,
 * which the design tokens in styles.scss key off of. 'system' removes the attribute
 * entirely so the prefers-color-scheme media query in styles.scss takes over. */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storage = inject(LocalStorageService);

  readonly preference = signal<ThemePreference>(this.storage.get<ThemePreference>(STORAGE_KEY) ?? 'system');

  constructor() {
    effect(() => {
      const pref = this.preference();
      const root = document.documentElement;

      if (pref === 'system') {
        root.removeAttribute('data-theme');
      } else {
        root.setAttribute('data-theme', pref);
      }

      this.storage.set(STORAGE_KEY, pref);
    });
  }

  setPreference(preference: ThemePreference): void {
    this.preference.set(preference);
  }

  cycle(): void {
    const order: ThemePreference[] = ['system', 'light', 'dark'];
    const next = order[(order.indexOf(this.preference()) + 1) % order.length];
    this.setPreference(next);
  }
}
