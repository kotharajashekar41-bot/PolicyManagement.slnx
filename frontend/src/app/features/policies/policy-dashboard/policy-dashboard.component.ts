import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { PolicyApiService } from '../../../core/services/policy-api.service';
import { LineOfBusiness, PolicyStatus } from '../../../core/models/policy.models';
import { PolicyFiltersComponent } from '../components/policy-filters/policy-filters.component';
import { PolicySummaryPanelComponent } from '../components/policy-summary-panel/policy-summary-panel.component';
import { PolicyTableComponent } from '../components/policy-table/policy-table.component';
import { BulkActionsBarComponent } from '../components/bulk-actions-bar/bulk-actions-bar.component';
import { PaginationControlsComponent } from '../components/pagination-controls/pagination-controls.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { ErrorStateComponent } from '../../../shared/components/error-state/error-state.component';
import { LoadingSkeletonComponent } from '../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PolicyQueryStore } from '../state/policy-query.store';
import { PolicyDataService } from '../state/policy-data.service';

/** The route's container component: owns wiring between PolicyQueryStore (client/URL
 * state), PolicyDataService (server state), and the presentational components below it.
 * PolicyQueryStore and PolicyDataService are provided HERE (route-scoped), not in root
 * — their state should reset if you navigate away and back, not persist app-wide. */
@Component({
  selector: 'app-policy-dashboard',
  standalone: true,
  imports: [
    PolicyFiltersComponent,
    PolicySummaryPanelComponent,
    PolicyTableComponent,
    BulkActionsBarComponent,
    PaginationControlsComponent,
    EmptyStateComponent,
    ErrorStateComponent,
    LoadingSkeletonComponent
  ],
  providers: [PolicyQueryStore, PolicyDataService],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './policy-dashboard.component.html',
  styleUrl: './policy-dashboard.component.scss'
})
export class PolicyDashboardComponent {
  protected readonly store = inject(PolicyQueryStore);
  protected readonly data = inject(PolicyDataService);
  private readonly api = inject(PolicyApiService);

  protected readonly isFlagging = signal(false);
  protected readonly flagError = signal<string | null>(null);

  protected onStatusChange(status: PolicyStatus | null): void {
    this.store.setStatus(status);
  }

  protected onLineOfBusinessChange(lob: LineOfBusiness | null): void {
    this.store.setLineOfBusiness(lob);
  }

  protected visiblePolicyIds(): string[] {
    return this.data.policies().map((p) => p.id);
  }

  protected onFlagSelected(): void {
    const ids = Array.from(this.store.selectedIds());
    if (ids.length === 0) return;

    this.isFlagging.set(true);
    this.flagError.set(null);

    this.api.flagForReview(ids).subscribe({
      next: () => {
        this.isFlagging.set(false);
        this.store.clearSelection();
        this.data.refetchList();
      },
      error: () => {
        this.isFlagging.set(false);
        this.flagError.set('Could not flag the selected policies. Please try again.');
      }
    });
  }
}
