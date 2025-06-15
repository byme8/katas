import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { Company } from '../models/company.model';
import { OffsetPaginationRequest, OffsetPaginationResponse } from '../models/pagination.model';

export interface RequestOptions {
  abortController?: AbortController;
}

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private readonly http = inject(HttpClient);

  getCompanies(options?: RequestOptions): Observable<Company[]> {
    if (options?.abortController) {
      return from(fetch('/api/companies', { 
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' }
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.get<Company[]>('/api/companies');
  }

  getCompaniesWithPagination(request: OffsetPaginationRequest, options?: RequestOptions): Observable<OffsetPaginationResponse<Company>> {
    if (options?.abortController) {
      return from(fetch('/api/companies/offset', {
        method: 'POST',
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.post<OffsetPaginationResponse<Company>>('/api/companies/offset', request);
  }
}