import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  DEFAULT_POLICY_QUERY,
  LineOfBusiness,
  PolicyQuery,
  PolicyStatus,
  SortDirection
} from '../../../core/models/policy.models';

/** Owns CLIENT/URL state only: the current filter/sort/page selection, and which rows
 * are selected for bulk-flag. It does not fetch data or hold server responses — that's
 * PolicyDataService — so "what the user is asking for" stays cleanly separate from
 * "what the server last returned for that ask".
 *
 * Filters round-trip through the URL query string (not just component state), so a
 * filtered/sorted/paged view is shareable and survives a refresh — provided per-route
 * (see PolicyDashboardComponent) rather than root, since its state is only meaningful
 * while that route is active. */
@Injectable()
export class PolicyQueryStore {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly initial = this.readFromUrl();

  readonly page = signal(this.initial.page);
  readonly size = signal(this.initial.size);
  readonly sortField = signal(this.initial.sortField);
  readonly sortDirection = signal<SortDirection>(this.initial.sortDirection);
  readonly status = signal<PolicyStatus | null>(this.initial.status);
  readonly lineOfBusiness = signal<LineOfBusiness | null>(this.initial.lineOfBusiness);
  readonly region = signal<string | null>(this.initial.region);
  readonly effectiveDateFrom = signal<string | null>(this.initial.effectiveDateFrom);
  readonly effectiveDateTo = signal<string | null>(this.initial.effectiveDateTo);
  readonly search = signal<string | null>(this.initial.search);

  readonly selectedIds = signal<ReadonlySet<string>>(new Set());

  readonly query = computed<PolicyQuery>(() => ({
    page: this.page(),
    size: this.size(),
    sortField: this.sortField(),
    sortDirection: this.sortDirection(),
    status: this.status(),
    lineOfBusiness: this.lineOfBusiness(),
    region: this.region(),
    effectiveDateFrom: this.effectiveDateFrom(),
    effectiveDateTo: this.effectiveDateTo(),
    search: this.search()
  }));

  /** Filter fields only (no page/size/sort) — lets PolicyDataService refetch the
   * summary panel only when a filter actually changes, not on every page turn. */
  readonly filtersOnly = computed(() => ({
    status: this.status(),
    lineOfBusiness: this.lineOfBusiness(),
    region: this.region(),
    effectiveDateFrom: this.effectiveDateFrom(),
    effectiveDateTo: this.effectiveDateTo(),
    search: this.search()
  }));

  readonly hasActiveFilters = computed(
    () =>
      this.status() !== null ||
      this.lineOfBusiness() !== null ||
      !!this.region() ||
      !!this.effectiveDateFrom() ||
      !!this.effectiveDateTo() ||
      !!this.search()
  );

  constructor() {
    // One-way: signals -> URL. We deliberately don't subscribe back to route changes
    // after init, so this can't loop; browser back/forward re-reads via readFromUrl()
    // only on a fresh navigation to this route (a full component re-create).
    effect(() => {
      const q = this.query();
      untracked(() =>
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: this.toQueryParams(q),
          replaceUrl: true
        })
      );
    });
  }

  setSort(field: string): void {
    if (this.sortField() === field) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortField.set(field);
      this.sortDirection.set('asc');
    }
    this.page.set(1);
  }

  setPage(page: number): void {
    this.page.set(page);
  }

  setStatus(status: PolicyStatus | null): void {
    this.status.set(status);
    this.page.set(1);
  }

  setLineOfBusiness(lob: LineOfBusiness | null): void {
    this.lineOfBusiness.set(lob);
    this.page.set(1);
  }

  setRegion(region: string | null): void {
    this.region.set(region || null);
    this.page.set(1);
  }

  setDateRange(from: string | null, to: string | null): void {
    this.effectiveDateFrom.set(from || null);
    this.effectiveDateTo.set(to || null);
    this.page.set(1);
  }

  setSearch(term: string | null): void {
    this.search.set(term?.trim() || null);
    this.page.set(1);
  }

  resetFilters(): void {
    this.status.set(null);
    this.lineOfBusiness.set(null);
    this.region.set(null);
    this.effectiveDateFrom.set(null);
    this.effectiveDateTo.set(null);
    this.search.set(null);
    this.page.set(1);
  }

  toggleSelection(id: string): void {
    const next = new Set(this.selectedIds());
    next.has(id) ? next.delete(id) : next.add(id);
    this.selectedIds.set(next);
  }

  toggleSelectAll(ids: string[]): void {
    const current = this.selectedIds();
    const allSelected = ids.length > 0 && ids.every((id) => current.has(id));
    this.selectedIds.set(allSelected ? new Set() : new Set(ids));
  }

  clearSelection(): void {
    this.selectedIds.set(new Set());
  }

  private readFromUrl(): PolicyQuery {
    const params = this.route.snapshot.queryParamMap;
    const [sortField, sortDirection] = (params.get('sort') ?? 'createdAt,desc').split(',');

    return {
      page: Number(params.get('page')) || DEFAULT_POLICY_QUERY.page,
      size: Number(params.get('size')) || DEFAULT_POLICY_QUERY.size,
      sortField: sortField || DEFAULT_POLICY_QUERY.sortField,
      sortDirection: sortDirection === 'asc' ? 'asc' : 'desc',
      status: (params.get('status') as PolicyStatus) || null,
      lineOfBusiness: (params.get('lineOfBusiness') as LineOfBusiness) || null,
      region: params.get('region'),
      effectiveDateFrom: params.get('effectiveDateFrom'),
      effectiveDateTo: params.get('effectiveDateTo'),
      search: params.get('search')
    };
  }

  private toQueryParams(q: PolicyQuery): Record<string, string | null> {
    return {
      page: q.page !== 1 ? String(q.page) : null,
      size: q.size !== DEFAULT_POLICY_QUERY.size ? String(q.size) : null,
      sort:
        q.sortField !== DEFAULT_POLICY_QUERY.sortField || q.sortDirection !== DEFAULT_POLICY_QUERY.sortDirection
          ? `${q.sortField},${q.sortDirection}`
          : null,
      status: q.status,
      lineOfBusiness: q.lineOfBusiness,
      region: q.region,
      effectiveDateFrom: q.effectiveDateFrom,
      effectiveDateTo: q.effectiveDateTo,
      search: q.search
    };
  }
}
