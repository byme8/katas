import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { UserService, RequestOptions } from '../../services/user.service';
import { User } from '../../models/user.model';
import { CursorPaginationRequest, CursorPaginationResponse } from '../../models/pagination.model';
import { CursorPaginationComponent, CursorPaginationConfig } from '../cursor-pagination/cursor-pagination.component';
import { USER_LIST_CONFIG, UserListUtils } from '../shared/user-list-config';

@Component({
  selector: 'app-user-list-cursor',
  standalone: true,
  imports: [CommonModule, CursorPaginationComponent, MatTableModule, MatSortModule, MatCardModule, MatProgressSpinnerModule, MatIconModule, MatFormFieldModule, MatSelectModule, MatButtonModule],
  templateUrl: './user-list-cursor.component.html',
  styleUrl: './user-list-cursor.component.scss'
})
export class UserListCursorComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  
  Math = Math;
  displayedColumns = USER_LIST_CONFIG.displayedColumns;
  userConfig = USER_LIST_CONFIG;
  
  // State signals
  title = signal('Users (Cursor Pagination)');
  paginationResponse = signal<CursorPaginationResponse<User> | null>(null);
  loading = signal(true);
  error = signal('');
  size = signal(25);
  orderBy = signal('CREATED_AT');
  direction = signal<'ASC' | 'DESC'>('DESC');
  currentCursor = signal<string | null>(null);
  
  
  // Download time tracking
  downloadStartTime = signal<number | null>(null);
  downloadEndTime = signal<number | null>(null);
  downloadDuration = computed(() => {
    const start = this.downloadStartTime();
    const end = this.downloadEndTime();
    return start && end ? end - start : null;
  });
  
  // Cancel loading
  private currentAbortController: AbortController | null = null;
  
  // Computed signals
  users = computed(() => this.paginationResponse()?.data ?? []);
  metadata = computed(() => this.paginationResponse()?.metadata ?? null);
  hasData = computed(() => this.users().length > 0);
  sortDirection = computed(() => this.direction().toLowerCase() as 'asc' | 'desc');
  sortActive = computed(() => UserListUtils.getSortActiveField(this.orderBy()));
  paginationConfig = computed((): CursorPaginationConfig => ({
    size: this.size()
  }));

  ngOnInit() {
    this.route.queryParams.subscribe(queryParams => {
      // Set size from query params or default to 25
      const size = queryParams['size'] ? +queryParams['size'] : 25;
      this.size.set(size);
      
      // Set orderBy from query params or default to 'CREATED_AT'
      const orderBy = queryParams['orderBy'] || 'CREATED_AT';
      this.orderBy.set(orderBy);
      
      // Set direction from query params or default to 'DESC'
      const direction = queryParams['direction'] || 'DESC';
      this.direction.set(direction as 'ASC' | 'DESC');
      
      // Set cursor from query params
      const cursor = queryParams['cursor'] || null;
      this.currentCursor.set(cursor);
      
      this.loadUsers();
    });
  }

  loadUsers() {
    // Cancel any ongoing request
    if (this.currentAbortController) {
      this.currentAbortController.abort();
    }
    
    this.currentAbortController = new AbortController();
    this.loading.set(true);
    this.error.set('');
    this.downloadStartTime.set(performance.now());
    this.downloadEndTime.set(null);
    
    const currentCursor = this.currentCursor();
    
    const request: CursorPaginationRequest = {
      size: this.size(),
      cursor: currentCursor || undefined,
      orderBy: this.orderBy(),
      direction: this.direction()
    };
    
    const options: RequestOptions = {
      abortController: this.currentAbortController
    };
    
    this.userService.getUsersWithCursorPagination(request, options).subscribe({
      next: (response) => {
        this.downloadEndTime.set(performance.now());
        this.paginationResponse.set(response);
        this.loading.set(false);
        this.currentAbortController = null;
      },
      error: (err) => {
        if (err.name === 'AbortError') {
          // Request was cancelled, don't update state
          return;
        }
        this.downloadEndTime.set(performance.now());
        this.error.set(err.message || 'An error occurred');
        this.loading.set(false);
        this.currentAbortController = null;
      }
    });
  }

  onNextPage() {
    const metadata = this.metadata();
    if (metadata?.hasNextPage && metadata.nextCursor) {
      this.updateUrl({ cursor: metadata.nextCursor });
    }
  }

  onPreviousPage() {
    const metadata = this.metadata();
    if (metadata?.hasPreviousPage && metadata.previousCursor) {
      this.updateUrl({ cursor: metadata.previousCursor });
    }
  }

  onPageSizeChange(size: number) {
    this.updateUrl({ size, cursor: null });
  }
  
  onSortChange(sort: Sort) {
    if (sort.active) {
      const newField = UserListUtils.getApiFieldName(sort.active);
      
      if (sort.direction) {
        // Normal case: set the field and direction
        this.updateUrl({ orderBy: newField, direction: sort.direction.toUpperCase() as 'ASC' | 'DESC', cursor: null });
      } else {
        // When direction is empty (third click), flip to opposite direction
        const currentDirection = this.direction();
        this.updateUrl({ orderBy: newField, direction: currentDirection === 'ASC' ? 'DESC' : 'ASC', cursor: null });
      }
    }
  }
  
  updateSize(value: number) {
    this.updateUrl({ size: value, cursor: null });
  }
  
  onFirstPage() {
    // Use the first page cursor from metadata for consistency
    const metadata = this.metadata();
    if (metadata?.firstPageCursor) {
      this.updateUrl({ cursor: metadata.firstPageCursor });
    } else {
      // Fallback to null cursor (traditional first page)
      this.updateUrl({ cursor: null });
    }
  }

  onLastPage() {
    // Use the last page cursor from metadata
    const metadata = this.metadata();
    if (metadata?.lastPageCursor) {
      this.updateUrl({ cursor: metadata.lastPageCursor });
    }
  }

  cancelLoading() {
    if (this.currentAbortController) {
      this.currentAbortController.abort();
      this.currentAbortController = null;
      this.loading.set(false);
    }
  }


  private updateUrl(params: { size?: number; orderBy?: string; direction?: 'ASC' | 'DESC'; cursor?: string | null }) {
    const currentParams = { ...this.route.snapshot.queryParams };
    
    // Update parameters
    if (params.size !== undefined) currentParams['size'] = params.size.toString();
    if (params.orderBy !== undefined) currentParams['orderBy'] = params.orderBy;
    if (params.direction !== undefined) currentParams['direction'] = params.direction;
    if (params.cursor !== undefined) {
      if (params.cursor) {
        currentParams['cursor'] = params.cursor;
      } else {
        delete currentParams['cursor'];
      }
    }
    
    // Remove default values to keep URL clean
    if (currentParams['size'] === '25') delete currentParams['size'];
    if (currentParams['orderBy'] === 'CREATED_AT') delete currentParams['orderBy'];
    if (currentParams['direction'] === 'DESC') delete currentParams['direction'];
    
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: currentParams,
      queryParamsHandling: 'replace'
    });
  }
}