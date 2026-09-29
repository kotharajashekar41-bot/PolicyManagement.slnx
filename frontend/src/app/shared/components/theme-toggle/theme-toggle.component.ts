import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ThemeService } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      class="theme-toggle"
      (click)="theme.cycle()"
      [attr.aria-label]="'Theme: ' + theme.preference() + '. Activate to change.'"
      [title]="'Theme: ' + theme.preference()"
    >
      <span aria-hidden="true">{{ icon() }}</span>
      <span class="theme-toggle__label">{{ theme.preference() }}</span>
    </button>
  `,
  styleUrl: './theme-toggle.component.scss'
})
export class ThemeToggleComponent {
  protected readonly theme = inject(ThemeService);

  protected icon(): string {
    switch (this.theme.preference()) {
      case 'light':
        return '☀️';
      case 'dark':
        return '🌙';
      default:
        return '🖥️';
    }
  }
}
