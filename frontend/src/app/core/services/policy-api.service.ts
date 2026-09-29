import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  FlagPoliciesResult,
  PagedResult,
  Policy,
  PolicyFilters,
  PolicyQuery,
  PolicySummary
} from '../models/policy.models';

/** Thin HTTP wrapper around the Policy API — no state, no caching, just requests
 * shaped to match openapi/policy-api.yaml. Keeps "how we talk to the server" separate
 * from "what the app currently wants" (that's PolicyQueryStore's job). */
@Injectable({ providedIn: 'root' })
export class PolicyApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/policies`;

  list(query: PolicyQuery): Observable<PagedResult<Policy>> {
    let params = this.toFilterParams(query);
    params = params.set('page', query.page).set('size', query.size);
    params = params.set('sort', `${query.sortField},${query.sortDirection}`);
    return this.http.get<PagedResult<Policy>>(this.baseUrl, { params });
  }

  summary(filters: PolicyFilters): Observable<PolicySummary> {
    return this.http.get<PolicySummary>(`${this.baseUrl}/summary`, { params: this.toFilterParams(filters) });
  }

  getById(id: string): Observable<Policy> {
    return this.http.get<Policy>(`${this.baseUrl}/${id}`);
  }

  flagForReview(policyIds: string[]): Observable<FlagPoliciesResult> {
    return this.http.patch<FlagPoliciesResult>(`${this.baseUrl}/flag`, { policyIds });
  }

  private toFilterParams(filters: PolicyFilters): HttpParams {
    let params = new HttpParams();

    if (filters.status) params = params.set('status', filters.status);
    if (filters.lineOfBusiness) params = params.set('lineOfBusiness', filters.lineOfBusiness);
    if (filters.region) params = params.set('region', filters.region);
    if (filters.effectiveDateFrom) params = params.set('effectiveDateFrom', filters.effectiveDateFrom);
    if (filters.effectiveDateTo) params = params.set('effectiveDateTo', filters.effectiveDateTo);
    if (filters.search) params = params.set('search', filters.search);

    return params;
  }
}
