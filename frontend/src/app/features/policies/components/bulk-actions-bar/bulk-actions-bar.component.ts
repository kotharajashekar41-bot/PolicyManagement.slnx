import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-bulk-actions-bar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (selectedCount() > 0) {
      <div class="bulk-actions" role="region" aria-label="Bulk actions">
        <span class="bulk-actions__count">{{ selectedCount() }} selected</span>
        <button type="button" class="bulk-actions__flag" [disabled]="flagging()" (click)="flagSelected.emit()">
          {{ flagging() ? 'Flagging…' : 'Flag for review' }}
        </button>
        <button type="button" class="bulk-actions__clear" (click)="clearSelection.emit()">Clear selection</button>
      </div>
    }
  `,
  styleUrl: './bulk-actions-bar.component.scss'
})
export class BulkActionsBarComponent {
  readonly selectedCount = input(0);
  readonly flagging = input(false);
  readonly flagSelected = output<void>();
  readonly clearSelection = output<void>();
}
