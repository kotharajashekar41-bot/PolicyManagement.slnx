import { TestBed } from '@angular/core/testing';
import { LocalStorageService } from './local-storage.service';

describe('LocalStorageService', () => {
  let service: LocalStorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(LocalStorageService);
  });

  afterEach(() => localStorage.clear());

  it('returns null for a key that was never set', () => {
    expect(service.get('missing-key')).toBeNull();
  });

  it('round-trips a stored value', () => {
    service.set('theme', { preference: 'dark' });
    expect(service.get<{ preference: string }>('theme')).toEqual({ preference: 'dark' });
  });

  it('removes a value', () => {
    service.set('k', 'v');
    service.remove('k');
    expect(service.get('k')).toBeNull();
  });

  it('returns null instead of throwing when the stored value is corrupt JSON', () => {
    localStorage.setItem('bad-key', '{not json');
    expect(service.get('bad-key')).toBeNull();
  });
});
