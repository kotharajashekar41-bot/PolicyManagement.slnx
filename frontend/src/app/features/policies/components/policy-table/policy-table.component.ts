import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Policy, SortDirection } from '../../../../core/models/policy.models';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

interface Column {
  field: string;
  label: string;
  sortable: boolean;
  numeric?: boolean;
}

const COLUMNS: Column[] = [
  { field: 'policyNumber', label: 'Policy #', sortable: true },
  { field: 'policyholderName', label: 'Policyholder', sortable: true },
  { field: 'lineOfBusiness', label: 'Line of business', sortable: true },
  { field: 'status', label: 'Status', sortable: true },
  { field: 'premiumAmount', label: 'Premium', sortable: true, numeric: true },
  { field: 'effectiveDate', label: 'Effective', sortable: true },
  { field: 'expiryDate', label: 'Expiry', sortable: true },
  { field: 'region', label: 'Region', sortable: true },
  { field: 'underwriter', label: 'Underwriter', sortable: true }
];

/** Purely presentational: renders whatever rows/selection it's given and reports user
 * intent (sort click, checkbox toggle) upward via outputs. Doesn't know about the query
 * store, HTTP, or routing — PolicyDashboardComponent owns all of that. */
@Component({
  selector: 'app-policy-table',
  standalone: true,
  imports: [StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './policy-table.component.html',
  styleUrl: './policy-table.component.scss'
})
export class PolicyTableComponent {
  readonly policies = input.required<Policy[]>();
  readonly sortField = input.required<string>();
  readonly sortDirection = input.required<SortDirection>();
  readonly selectedIds = input.required<ReadonlySet<string>>();

  readonly sortChange = output<string>();
  readonly toggleSelect = output<string>();
  readonly toggleSelectAll = output<void>();

  protected readonly columns = COLUMNS;

  protected readonly allSelected = computed(() => {
    const ids = this.policies().map((p) => p.id);
    return ids.length > 0 && ids.every((id) => this.selectedIds().has(id));
  });

  protected readonly someSelected = computed(
    () => !this.allSelected() && this.policies().some((p) => this.selectedIds().has(p.id))
  );

  protected ariaSortFor(field: string): 'ascending' | 'descending' | 'none' {
    if (this.sortField() !== field) return 'none';
    return this.sortDirection() === 'asc' ? 'ascending' : 'descending';
  }

  protected formatCurrency(amount: number, currency: string): string {
    try {
      return new Intl.NumberFormat('en', { style: 'currency', currency, maximumFractionDigits: 0 }).format(amount);
    } catch {
      return `${currency} ${amount.toFixed(0)}`;
    }
  }

  protected formatDate(iso: string): string {
    const date = new Date(iso);
    return Number.isNaN(date.getTime()) ? iso : date.toLocaleDateString('en', { dateStyle: 'medium' });
  }
}
