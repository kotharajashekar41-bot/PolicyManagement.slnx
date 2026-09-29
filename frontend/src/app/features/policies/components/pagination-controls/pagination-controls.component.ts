import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination-controls',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './pagination-controls.component.html',
  styleUrl: './pagination-controls.component.scss'
})
export class PaginationControlsComponent {
  readonly page = input.required<number>();
  readonly size = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly totalPages = input.required<number>();

  readonly pageChange = output<number>();

  protected readonly rangeStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.size() + 1
  );
  protected readonly rangeEnd = computed(() => Math.min(this.page() * this.size(), this.totalCount()));

  protected readonly canGoPrevious = computed(() => this.page() > 1);
  protected readonly canGoNext = computed(() => this.page() < this.totalPages());

  goTo(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.pageChange.emit(page);
    }
  }
}
