import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { PolicyFiltersComponent } from './policy-filters.component';

describe('PolicyFiltersComponent', () => {
  let fixture: ComponentFixture<PolicyFiltersComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [PolicyFiltersComponent] });
    fixture = TestBed.createComponent(PolicyFiltersComponent);
    fixture.detectChanges();
  });

  it('does not show the "Clear filters" button when no filters are active', () => {
    fixture.componentRef.setInput('hasActiveFilters', false);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.filters__reset')).toBeNull();
  });

  it('shows the "Clear filters" button when a filter is active', () => {
    fixture.componentRef.setInput('hasActiveFilters', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.filters__reset')).not.toBeNull();
  });

  it('emits reset when "Clear filters" is clicked', () => {
    fixture.componentRef.setInput('hasActiveFilters', true);
    fixture.detectChanges();
    let resetCalled = false;
    fixture.componentInstance.reset.subscribe(() => (resetCalled = true));

    (fixture.nativeElement.querySelector('.filters__reset') as HTMLButtonElement).click();

    expect(resetCalled).toBe(true);
  });

  // These two exercise the component's own handler methods directly rather than
  // round-tripping through <select>/<input> + ngModel's DOM event wiring, which is
  // framework machinery, not this component's logic, and proved flaky to drive via
  // dispatchEvent() in headless Karma. The debounce timing and status-mapping logic
  // below is what's actually specific to this component.
  it('debounces search input and emits once after 300ms of inactivity', fakeAsync(() => {
    const emitted: string[] = [];
    fixture.componentInstance.searchChange.subscribe((v) => emitted.push(v));

    fixture.componentInstance.onSearchInput('PO');
    tick(100);
    fixture.componentInstance.onSearchInput('POL');
    tick(299);
    expect(emitted).toEqual([]);

    tick(1);
    expect(emitted).toEqual(['POL']);
  }));

  it('maps the empty "All statuses" option value to null', () => {
    const emitted: (string | null)[] = [];
    fixture.componentInstance.statusChange.subscribe((v) => emitted.push(v));

    fixture.componentInstance.onStatusChange('');

    expect(emitted).toEqual([null]);
  });
});
