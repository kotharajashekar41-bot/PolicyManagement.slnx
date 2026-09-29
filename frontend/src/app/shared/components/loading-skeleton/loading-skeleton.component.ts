import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="skeleton" role="status" aria-label="Loading">
      @for (row of rowsArray(); track $index) {
        <div class="skeleton__row"></div>
      }
      <span class="visually-hidden">Loading…</span>
    </div>
  `,
  styleUrl: './loading-skeleton.component.scss'
})
export class LoadingSkeletonComponent {
  readonly rows = input(6);
  protected readonly rowsArray = () => Array.from({ length: this.rows() });
}
