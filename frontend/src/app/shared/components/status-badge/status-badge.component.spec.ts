import { ComponentFixture, TestBed } from '@angular/core/testing';
import { StatusBadgeComponent } from './status-badge.component';

describe('StatusBadgeComponent', () => {
  let fixture: ComponentFixture<StatusBadgeComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [StatusBadgeComponent] });
    fixture = TestBed.createComponent(StatusBadgeComponent);
  });

  it('renders the status text and a matching data-status attribute', () => {
    fixture.componentRef.setInput('status', 'Active');
    fixture.detectChanges();

    const badge = fixture.nativeElement.querySelector('.badge');
    expect(badge.textContent.trim()).toBe('Active');
    expect(badge.getAttribute('data-status')).toBe('Active');
  });
});
