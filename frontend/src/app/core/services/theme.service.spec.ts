import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  let service: ThemeService;

  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    TestBed.configureTestingModule({});
    service = TestBed.inject(ThemeService);
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
  });

  it('defaults to system preference when nothing is stored', () => {
    expect(service.preference()).toBe('system');
  });

  it('sets data-theme on <html> when an explicit preference is chosen', () => {
    service.setPreference('dark');
    TestBed.flushEffects();
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('removes data-theme when preference is system', () => {
    service.setPreference('light');
    TestBed.flushEffects();
    service.setPreference('system');
    TestBed.flushEffects();
    expect(document.documentElement.hasAttribute('data-theme')).toBe(false);
  });

  it('persists the preference to localStorage', () => {
    service.setPreference('dark');
    TestBed.flushEffects();
    expect(localStorage.getItem('policyplatform.theme')).toBe('"dark"');
  });

  it('restores a persisted preference on next construction', () => {
    localStorage.setItem('policyplatform.theme', '"dark"');
    const fresh = TestBed.runInInjectionContext(() => new ThemeService());
    expect(fresh.preference()).toBe('dark');
  });

  it('cycle() rotates system -> light -> dark -> system', () => {
    expect(service.preference()).toBe('system');
    service.cycle();
    expect(service.preference()).toBe('light');
    service.cycle();
    expect(service.preference()).toBe('dark');
    service.cycle();
    expect(service.preference()).toBe('system');
  });
});
