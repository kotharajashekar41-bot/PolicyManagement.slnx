import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { DEFAULT_POLICY_QUERY } from '../models/policy.models';
import { PolicyApiService } from './policy-api.service';

describe('PolicyApiService', () => {
  let service: PolicyApiService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/policies`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(PolicyApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('list() requests page/size/sort and omits unset filters', () => {
    service.list(DEFAULT_POLICY_QUERY).subscribe();

    const req = httpMock.expectOne(
      (r) => r.url === baseUrl && r.params.get('page') === '1' && r.params.get('size') === '20'
    );
    expect(req.request.params.get('sort')).toBe('createdAt,desc');
    expect(req.request.params.has('status')).toBe(false);
    expect(req.request.params.has('search')).toBe(false);
    req.flush({ items: [], page: 1, size: 20, totalCount: 0, totalPages: 0 });
  });

  it('list() includes active filters as query params', () => {
    service
      .list({ ...DEFAULT_POLICY_QUERY, status: 'Active', lineOfBusiness: 'A&H', search: 'POL-1' })
      .subscribe();

    const req = httpMock.expectOne((r) => r.url === baseUrl);
    expect(req.request.params.get('status')).toBe('Active');
    expect(req.request.params.get('lineOfBusiness')).toBe('A&H');
    expect(req.request.params.get('search')).toBe('POL-1');
    req.flush({ items: [], page: 1, size: 20, totalCount: 0, totalPages: 0 });
  });

  it('summary() does not include paging or sort params', () => {
    service.summary({ status: 'Active', lineOfBusiness: null, region: null, effectiveDateFrom: null, effectiveDateTo: null, search: null }).subscribe();

    const req = httpMock.expectOne((r) => r.url === `${baseUrl}/summary`);
    expect(req.request.params.has('page')).toBe(false);
    expect(req.request.params.has('sort')).toBe(false);
    expect(req.request.params.get('status')).toBe('Active');
    req.flush({ countsByStatus: {}, premiumByLineOfBusiness: {}, expiringSoonCount: 0 });
  });

  it('flagForReview() PATCHes the id array as the request body', () => {
    service.flagForReview(['id-1', 'id-2']).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/flag`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ policyIds: ['id-1', 'id-2'] });
    req.flush({ flaggedCount: 2 });
  });

  it('getById() requests the single-policy endpoint', () => {
    service.getById('abc').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/abc`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});
