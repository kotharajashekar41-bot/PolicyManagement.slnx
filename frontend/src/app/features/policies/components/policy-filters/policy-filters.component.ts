import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LINES_OF_BUSINESS,
  LineOfBusiness,
  POLICY_STATUSES,
  PolicyStatus
} from '../../../../core/models/policy.models';

/** Purely presentational: renders the current filter values it's given and emits an
 * event per field change. Owns no state of its own (beyond the debounce timer for
 * search) — PolicyDashboardComponent wires these events to PolicyQueryStore, so this
 * component stays testable without a Router/store in the harness. */
@Component({
  selector: 'app-policy-filters',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './policy-filters.component.html',
  styleUrl: './policy-filters.component.scss'
})
export class PolicyFiltersComponent {
  readonly search = input<string | null>(null);
  readonly status = input<PolicyStatus | null>(null);
  readonly lineOfBusiness = input<LineOfBusiness | null>(null);
  readonly region = input<string | null>(null);
  readonly effectiveDateFrom = input<string | null>(null);
  readonly effectiveDateTo = input<string | null>(null);
  readonly hasActiveFilters = input(false);

  readonly searchChange = output<string>();
  readonly statusChange = output<PolicyStatus | null>();
  readonly lineOfBusinessChange = output<LineOfBusiness | null>();
  readonly regionChange = output<string | null>();
  readonly dateRangeChange = output<{ from: string | null; to: string | null }>();
  readonly reset = output<void>();

  protected readonly statuses = POLICY_STATUSES;
  protected readonly linesOfBusiness = LINES_OF_BUSINESS;

  private searchDebounce?: ReturnType<typeof setTimeout>;

  protected readonly searchModel = computed(() => this.search() ?? '');

  onSearchInput(value: string): void {
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => this.searchChange.emit(value), 300);
  }

  onStatusChange(value: string): void {
    this.statusChange.emit((value || null) as PolicyStatus | null);
  }

  onLineOfBusinessChange(value: string): void {
    this.lineOfBusinessChange.emit((value || null) as LineOfBusiness | null);
  }

  onRegionChange(value: string): void {
    this.regionChange.emit(value || null);
  }

  onDateFromChange(value: string): void {
    this.dateRangeChange.emit({ from: value || null, to: this.effectiveDateTo() });
  }

  onDateToChange(value: string): void {
    this.dateRangeChange.emit({ from: this.effectiveDateFrom(), to: value || null });
  }
}
