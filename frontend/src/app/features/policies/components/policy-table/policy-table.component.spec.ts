import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Policy } from '../../../../core/models/policy.models';
import { PolicyTableComponent } from './policy-table.component';

const POLICIES: Policy[] = [
  {
    id: '1',
    policyNumber: 'POL-000001',
    policyholderName: 'Alice Tan',
    lineOfBusiness: 'Property',
    status: 'Active',
    premiumAmount: 10000,
    currency: 'SGD',
    effectiveDate: '2026-01-01',
    expiryDate: '2027-01-01',
    region: 'Singapore',
    underwriter: 'UW One',
    flaggedForReview: false,
    createdAt: '2026-01-01T00:00:00Z',
    updatedAt: '2026-01-01T00:00:00Z'
  },
  {
    id: '2',
    policyNumber: 'POL-000002',
    policyholderName: 'Bob Lim',
    lineOfBusiness: 'Marine',
    status: 'Expired',
    premiumAmount: 25000,
    currency: 'HKD',
    effectiveDate: '2024-01-01',
    expiryDate: '2025-01-01',
    region: 'Hong Kong',
    underwriter: 'UW Two',
    flaggedForReview: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z'
  }
];

describe('PolicyTableComponent', () => {
  let fixture: ComponentFixture<PolicyTableComponent>;

  function setup(selected: string[] = []) {
    fixture = TestBed.createComponent(PolicyTableComponent);
    fixture.componentRef.setInput('policies', POLICIES);
    fixture.componentRef.setInput('sortField', 'policyNumber');
    fixture.componentRef.setInput('sortDirection', 'asc');
    fixture.componentRef.setInput('selectedIds', new Set(selected));
    fixture.detectChanges();
  }

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [PolicyTableComponent] });
  });

  it('renders one row per policy', () => {
    setup();
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(2);
  });

  it('marks the currently-sorted column header with aria-sort', () => {
    setup();
    const headers: HTMLTableCellElement[] = Array.from(fixture.nativeElement.querySelectorAll('th[scope="col"]'));
    const sorted = headers.find((h) => h.textContent?.includes('Policy #'));
    expect(sorted?.getAttribute('aria-sort')).toBe('ascending');
  });

  it('emits sortChange with the clicked column field', () => {
    setup();
    const emitted: string[] = [];
    fixture.componentInstance.sortChange.subscribe((f) => emitted.push(f));

    const buttons: HTMLButtonElement[] = Array.from(fixture.nativeElement.querySelectorAll('.sort-button'));
    const premiumHeader = buttons.find((b) => b.textContent?.includes('Premium'))!;
    premiumHeader.click();

    expect(emitted).toEqual(['premiumAmount']);
  });

  it('emits toggleSelect with the row id when its checkbox is toggled', () => {
    setup();
    const emitted: string[] = [];
    fixture.componentInstance.toggleSelect.subscribe((id) => emitted.push(id));

    const rowCheckbox = fixture.nativeElement.querySelector(
      'tbody tr:first-child input[type="checkbox"]'
    ) as HTMLInputElement;
    rowCheckbox.click();

    expect(emitted).toEqual(['1']);
  });

  it('reflects selection state on row checkboxes', () => {
    setup(['2']);
    const checkboxes: HTMLInputElement[] = Array.from(
      fixture.nativeElement.querySelectorAll('tbody input[type="checkbox"]')
    );
    expect(checkboxes[0].checked).toBe(false);
    expect(checkboxes[1].checked).toBe(true);
  });

  it('shows the select-all checkbox as checked only when every visible row is selected', () => {
    setup(['1', '2']);
    const selectAll = fixture.nativeElement.querySelector('thead input[type="checkbox"]') as HTMLInputElement;
    expect(selectAll.checked).toBe(true);
  });

  it('shows a flag indicator for flagged policies', () => {
    setup();
    const flagIcons = fixture.nativeElement.querySelectorAll('.flag-indicator');
    expect(flagIcons.length).toBe(1);
  });

  it('emits toggleSelectAll when the header checkbox is toggled', () => {
    setup();
    let emitted = 0;
    fixture.componentInstance.toggleSelectAll.subscribe(() => emitted++);

    (fixture.nativeElement.querySelector('thead input[type="checkbox"]') as HTMLInputElement).click();

    expect(emitted).toBe(1);
  });
});
