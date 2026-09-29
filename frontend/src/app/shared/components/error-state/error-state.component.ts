import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-error-state',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="error-state" role="alert">
      <p class="error-state__title">Something went wrong</p>
      <p class="error-state__message">{{ message() }}</p>
      <button type="button" class="error-state__action" (click)="retry.emit()">Try again</button>
    </div>
  `,
  styleUrl: './error-state.component.scss'
})
export class ErrorStateComponent {
  readonly message = input('An unexpected error occurred.');
  readonly retry = output<void>();
}
