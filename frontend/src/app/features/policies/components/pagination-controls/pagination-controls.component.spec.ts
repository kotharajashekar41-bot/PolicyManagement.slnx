import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaginationControlsComponent } from './pagination-controls.component';

describe('PaginationControlsComponent', () => {
  let fixture: ComponentFixture<PaginationControlsComponent>;

  function setup(page: number, size: number, totalCount: number, totalPages: number) {
    fixture = TestBed.createComponent(PaginationControlsComponent);
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('size', size);
    fixture.componentRef.setInput('totalCount', totalCount);
    fixture.componentRef.setInput('totalPages', totalPages);
    fixture.detectChanges();
  }

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [PaginationControlsComponent] });
  });

  it('disables Previous on the first page', () => {
    setup(1, 20, 100, 5);
    const prev = fixture.nativeElement.querySelector('[aria-label="Previous page"]') as HTMLButtonElement;
    expect(prev.disabled).toBe(true);
  });

  it('disables Next on the last page', () => {
    setup(5, 20, 100, 5);
    const next = fixture.nativeElement.querySelector('[aria-label="Next page"]') as HTMLButtonElement;
    expect(next.disabled).toBe(true);
  });

  it('emits pageChange with the next page number when Next is clicked', () => {
    setup(2, 20, 100, 5);
    const emitted: number[] = [];
    fixture.componentInstance.pageChange.subscribe((p) => emitted.push(p));

    (fixture.nativeElement.querySelector('[aria-label="Next page"]') as HTMLButtonElement).click();

    expect(emitted).toEqual([3]);
  });

  it('shows the correct visible-range summary', () => {
    setup(2, 20, 45, 3);
    const summary = fixture.nativeElement.querySelector('.pagination__summary').textContent;
    expect(summary).toContain('21');
    expect(summary).toContain('40');
    expect(summary).toContain('45');
  });

  it('shows "No results" when totalCount is zero', () => {
    setup(1, 20, 0, 0);
    expect(fixture.nativeElement.querySelector('.pagination__summary').textContent).toContain('No results');
  });
});
