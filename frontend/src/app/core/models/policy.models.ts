export type PolicyStatus = 'Active' | 'Expired' | 'Pending' | 'Cancelled';

export type LineOfBusiness = 'Property' | 'Casualty' | 'A&H' | 'Marine';

export const POLICY_STATUSES: PolicyStatus[] = ['Active', 'Expired', 'Pending', 'Cancelled'];

export const LINES_OF_BUSINESS: LineOfBusiness[] = ['Property', 'Casualty', 'A&H', 'Marine'];

export interface Policy {
  id: string;
  policyNumber: string;
  policyholderName: string;
  lineOfBusiness: LineOfBusiness;
  status: PolicyStatus;
  premiumAmount: number;
  currency: string;
  effectiveDate: string;
  expiryDate: string;
  region: string;
  underwriter: string;
  flaggedForReview: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  size: number;
  totalCount: number;
  totalPages: number;
}

export interface PolicySummary {
  countsByStatus: Record<string, number>;
  premiumByLineOfBusiness: Record<string, number>;
  expiringSoonCount: number;
}

export interface FlagPoliciesResult {
  flaggedCount: number;
}

export type SortDirection = 'asc' | 'desc';

export interface PolicyFilters {
  status: PolicyStatus | null;
  lineOfBusiness: LineOfBusiness | null;
  region: string | null;
  effectiveDateFrom: string | null;
  effectiveDateTo: string | null;
  search: string | null;
}

export interface PolicyQuery extends PolicyFilters {
  page: number;
  size: number;
  sortField: string;
  sortDirection: SortDirection;
}

export const DEFAULT_POLICY_QUERY: PolicyQuery = {
  page: 1,
  size: 20,
  sortField: 'createdAt',
  sortDirection: 'desc',
  status: null,
  lineOfBusiness: null,
  region: null,
  effectiveDateFrom: null,
  effectiveDateTo: null,
  search: null
};
