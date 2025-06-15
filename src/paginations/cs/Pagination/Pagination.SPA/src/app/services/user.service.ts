import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from } from 'rxjs';
import { User } from '../models/user.model';
import { OffsetPaginationRequest, OffsetPaginationResponse, CursorPaginationRequest, CursorPaginationResponse } from '../models/pagination.model';

export interface RequestOptions {
  abortController?: AbortController;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly http = inject(HttpClient);

  getAllUsers(options?: RequestOptions): Observable<User[]> {
    if (options?.abortController) {
      return from(fetch('/api/users/all', { 
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' }
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.get<User[]>('/api/users/all');
  }

  getUsersWithPagination(request: OffsetPaginationRequest, options?: RequestOptions): Observable<OffsetPaginationResponse<User>> {
    if (options?.abortController) {
      return from(fetch('/api/users/offset', {
        method: 'POST',
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.post<OffsetPaginationResponse<User>>('/api/users/offset', request);
  }

  getUsersByCompany(companyId: number, options?: RequestOptions): Observable<User[]> {
    if (options?.abortController) {
      return from(fetch(`/api/companies/${companyId}/users`, {
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' }
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.get<User[]>(`/api/companies/${companyId}/users`);
  }

  getUsersByCompanyWithPagination(companyId: number, request: OffsetPaginationRequest, options?: RequestOptions): Observable<OffsetPaginationResponse<User>> {
    if (options?.abortController) {
      return from(fetch(`/api/companies/${companyId}/users/offset`, {
        method: 'POST',
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.post<OffsetPaginationResponse<User>>(`/api/companies/${companyId}/users/offset`, request);
  }

  getUsersWithCursorPagination(request: CursorPaginationRequest, options?: RequestOptions): Observable<CursorPaginationResponse<User>> {
    if (options?.abortController) {
      return from(fetch('/api/users/cursor', {
        method: 'POST',
        signal: options.abortController.signal,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      }).then(response => {
        if (!response.ok) throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        return response.json();
      }));
    }
    return this.http.post<CursorPaginationResponse<User>>('/api/users/cursor', request);
  }

}