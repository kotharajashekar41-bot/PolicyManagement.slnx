import { toObservable } from '@angular/core/rxjs-interop';
import { Injectable, computed, inject, signal } from '@angular/core';
import { EMPTY, catchError, switchMap, tap } from 'rxjs';
import { PolicyApiService } from '../../../core/services/policy-api.service';
import { PagedResult, Policy, PolicySummary } from '../../../core/models/policy.models';
import { PolicyQueryStore } from './policy-query.store';

/** Owns SERVER state: the last-fetched page of policies, the summary panel data, and
 * loading/error flags for each — kept apart from PolicyQueryStore's client/URL state so
 * "what changed" is always unambiguous (a filter edit vs. a response arriving). Refetches
 * reactively off the store's signals via toObservable + switchMap, so a rapid string of
 * filter edits only ever keeps the latest in-flight request's result. */
@Injectable()
export class PolicyDataService {
  private readonly api = inject(PolicyApiService);
  private readonly store = inject(PolicyQueryStore);

  private readonly listResult = signal<PagedResult<Policy> | null>(null);
  private readonly listLoading = signal(false);
  private readonly listError = signal<string | null>(null);

  private readonly summaryResult = signal<PolicySummary | null>(null);
  private readonly summaryLoading = signal(false);
  private readonly summaryError = signal<string | null>(null);

  readonly policies = computed(() => this.listResult()?.items ?? []);
  readonly totalCount = computed(() => this.listResult()?.totalCount ?? 0);
  readonly totalPages = computed(() => this.listResult()?.totalPages ?? 0);
  readonly isListLoading = this.listLoading.asReadonly();
  readonly listErrorMessage = this.listError.asReadonly();

  readonly summary = this.summaryResult.asReadonly();
  readonly isSummaryLoading = this.summaryLoading.asReadonly();
  readonly summaryErrorMessage = this.summaryError.asReadonly();

  constructor() {
    toObservable(this.store.query)
      .pipe(
        tap(() => {
          this.listLoading.set(true);
          this.listError.set(null);
        }),
        switchMap((query) =>
          this.api.list(query).pipe(
            catchError(() => {
              this.listError.set('Could not load policies. Check your connection and try again.');
              this.listLoading.set(false);
              return EMPTY;
            })
          )
        )
      )
      .subscribe((result) => {
        this.listResult.set(result);
        this.listLoading.set(false);
      });

    toObservable(this.store.filtersOnly)
      .pipe(
        tap(() => {
          this.summaryLoading.set(true);
          this.summaryError.set(null);
        }),
        switchMap((filters) =>
          this.api.summary(filters).pipe(
            catchError(() => {
              this.summaryError.set('Could not load summary statistics.');
              this.summaryLoading.set(false);
              return EMPTY;
            })
          )
        )
      )
      .subscribe((result) => {
        this.summaryResult.set(result);
        this.summaryLoading.set(false);
      });
  }

  /** Re-runs the current list query — used after a bulk flag so the table reflects it. */
  refetchList(): void {
    this.listLoading.set(true);
    this.listError.set(null);
    this.api.list(this.store.query()).subscribe({
      next: (result) => {
        this.listResult.set(result);
        this.listLoading.set(false);
      },
      error: () => {
        this.listError.set('Could not load policies. Check your connection and try again.');
        this.listLoading.set(false);
      }
    });
  }
}
