import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { PolicyStatus } from '../../../core/models/policy.models';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [attr.data-status]="status()">{{ status() }}</span>`,
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent {
  readonly status = input.required<PolicyStatus>();
}
