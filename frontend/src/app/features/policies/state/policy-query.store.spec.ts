import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, Router, convertToParamMap } from '@angular/router';
import { PolicyQueryStore } from './policy-query.store';

function createStore(initialParams: Record<string, string> = {}) {
  const navigate = jasmine.createSpy('navigate').and.returnValue(Promise.resolve(true));

  TestBed.configureTestingModule({
    providers: [
      PolicyQueryStore,
      { provide: Router, useValue: { navigate } },
      {
        provide: ActivatedRoute,
        useValue: { snapshot: { queryParamMap: convertToParamMap(initialParams) } }
      }
    ]
  });

  return { store: TestBed.inject(PolicyQueryStore), navigate };
}

describe('PolicyQueryStore', () => {
  it('defaults to page 1, size 20, sort createdAt desc, no filters', () => {
    const { store } = createStore();

    expect(store.query()).toEqual({
      page: 1,
      size: 20,
      sortField: 'createdAt',
      sortDirection: 'desc',
      status: null,
      lineOfBusiness: null,
      region: null,
      effectiveDateFrom: null,
      effectiveDateTo: null,
      search: null
    });
  });

  it('reads initial state from the URL query params', () => {
    const { store } = createStore({ page: '3', size: '50', sort: 'premiumAmount,asc', status: 'Active' });

    expect(store.page()).toBe(3);
    expect(store.size()).toBe(50);
    expect(store.sortField()).toBe('premiumAmount');
    expect(store.sortDirection()).toBe('asc');
    expect(store.status()).toBe('Active');
  });

  it('setSort toggles direction when the same field is clicked twice', () => {
    const { store } = createStore();

    store.setSort('premiumAmount');
    expect(store.sortField()).toBe('premiumAmount');
    expect(store.sortDirection()).toBe('asc');

    store.setSort('premiumAmount');
    expect(store.sortDirection()).toBe('desc');
  });

  it('setSort on a new field resets direction to asc', () => {
    const { store } = createStore();
    store.setSort('premiumAmount');
    store.setSort('premiumAmount'); // now desc

    store.setSort('region');
    expect(store.sortField()).toBe('region');
    expect(store.sortDirection()).toBe('asc');
  });

  it('any filter change resets page to 1', () => {
    const { store } = createStore();
    store.setPage(4);
    expect(store.page()).toBe(4);

    store.setStatus('Active');
    expect(store.page()).toBe(1);
  });

  it('resetFilters clears every filter field but leaves paging/sort alone', () => {
    const { store } = createStore();
    store.setStatus('Active');
    store.setLineOfBusiness('Marine');
    store.setRegion('Singapore');
    store.setSearch('POL-1');
    store.setSort('premiumAmount');

    store.resetFilters();

    expect(store.status()).toBeNull();
    expect(store.lineOfBusiness()).toBeNull();
    expect(store.region()).toBeNull();
    expect(store.search()).toBeNull();
    expect(store.sortField()).toBe('premiumAmount'); // untouched by resetFilters
  });

  it('hasActiveFilters reflects whether any filter is set', () => {
    const { store } = createStore();
    expect(store.hasActiveFilters()).toBe(false);

    store.setSearch('foo');
    expect(store.hasActiveFilters()).toBe(true);
  });

  describe('selection', () => {
    it('toggleSelection adds then removes an id', () => {
      const { store } = createStore();
      store.toggleSelection('a');
      expect(store.selectedIds().has('a')).toBe(true);

      store.toggleSelection('a');
      expect(store.selectedIds().has('a')).toBe(false);
    });

    it('toggleSelectAll selects all given ids, then deselects all on a second call', () => {
      const { store } = createStore();
      store.toggleSelectAll(['a', 'b', 'c']);
      expect(store.selectedIds().size).toBe(3);

      store.toggleSelectAll(['a', 'b', 'c']);
      expect(store.selectedIds().size).toBe(0);
    });

    it('clearSelection empties the selection', () => {
      const { store } = createStore();
      store.toggleSelection('a');
      store.clearSelection();
      expect(store.selectedIds().size).toBe(0);
    });
  });

  it('writes the current query to the URL via router.navigate as signals change', () => {
    const { store, navigate } = createStore();

    store.setSearch('POL-42');
    TestBed.flushEffects();

    expect(navigate).toHaveBeenCalled();
    const [, options] = navigate.calls.mostRecent().args;
    expect(options.queryParams.search).toBe('POL-42');
  });
});
