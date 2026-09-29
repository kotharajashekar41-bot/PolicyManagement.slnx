import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="empty-state" role="status">
      <p class="empty-state__title">{{ title() }}</p>
      <p class="empty-state__hint">{{ hint() }}</p>
      @if (showReset()) {
        <button type="button" class="empty-state__action" (click)="reset.emit()">Clear filters</button>
      }
    </div>
  `,
  styleUrl: './empty-state.component.scss'
})
export class EmptyStateComponent {
  readonly title = input('No policies found');
  readonly hint = input('Try adjusting your filters or search term.');
  readonly showReset = input(false);
  readonly reset = output<void>();
}
