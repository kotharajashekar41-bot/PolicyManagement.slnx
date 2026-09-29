import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { PolicySummary } from '../../../../core/models/policy.models';

interface StatTile {
  label: string;
  value: string;
  emphasis?: 'warning';
}

/** Purely presentational stat-tile row. Formats the raw PolicySummary into display
 * strings once via computed(), rather than re-formatting in the template on every
 * change-detection pass. */
@Component({
  selector: 'app-policy-summary-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './policy-summary-panel.component.html',
  styleUrl: './policy-summary-panel.component.scss'
})
export class PolicySummaryPanelComponent {
  readonly summary = input<PolicySummary | null>(null);
  readonly loading = input(false);

  protected readonly statusTiles = computed<StatTile[]>(() => {
    const counts = this.summary()?.countsByStatus ?? {};
    return Object.entries(counts).map(([status, count]) => ({
      label: status,
      value: count.toLocaleString()
    }));
  });

  protected readonly premiumTiles = computed<StatTile[]>(() => {
    const premiums = this.summary()?.premiumByLineOfBusiness ?? {};
    return Object.entries(premiums).map(([lob, total]) => ({
      label: lob === 'AH' ? 'A&H' : lob,
      value: this.formatCurrency(total)
    }));
  });

  protected readonly expiringSoonCount = computed(() => this.summary()?.expiringSoonCount ?? 0);

  private formatCurrency(value: number): string {
    if (value >= 1_000_000) return `${(value / 1_000_000).toFixed(1)}M`;
    if (value >= 1_000) return `${(value / 1_000).toFixed(0)}K`;
    return value.toFixed(0);
  }
}
