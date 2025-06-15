import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { UserService, RequestOptions } from '../../services/user.service';
import { User } from '../../models/user.model';
import { OffsetPaginationRequest, OffsetPaginationResponse } from '../../models/pagination.model';
import { OffsetPaginationComponent, PaginationConfig } from '../offset-pagination/offset-pagination.component';
import { skipCountCalculation } from '../../stores/settings.store';
import { USER_LIST_CONFIG, UserListUtils } from '../shared/user-list-config';

interface CachedPage {
  request: OffsetPaginationRequest;
  response: OffsetPaginationResponse<User>;
  timestamp: number;
}

@Component({
  selector: 'app-user-list-precache',
  standalone: true,
  imports: [
    CommonModule, 
    OffsetPaginationComponent, 
    MatTableModule, 
    MatSortModule, 
    MatCardModule, 
    MatProgressSpinnerModule, 
    MatIconModule, 
    MatFormFieldModule, 
    MatSelectModule,
    MatButtonModule,
    MatChipsModule
  ],
  templateUrl: './user-list-precache.component.html',
  styleUrl: './user-list-precache.component.scss'
})
export class UserListPrecacheComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly route = inject(ActivatedRoute);
  
  Math = Math;
  displayedColumns = USER_LIST_CONFIG.displayedColumns;
  userConfig = USER_LIST_CONFIG;
  
  // State signals
  title = signal('Users (With Precaching)');
  companyId = signal<number | null>(null);
  paginationResponse = signal<OffsetPaginationResponse<User> | null>(null);
  loading = signal(true);
  error = signal('');
  page = signal(1);
  size = signal(25);
  orderBy = signal('CREATED_AT');
  direction = signal<'ASC' | 'DESC'>('DESC');
  
  // Precaching state
  precacheLoading = signal(false);
  pageCache = signal<Map<string, CachedPage>>(new Map());
  
  // Download time tracking
  downloadStartTime = signal<number | null>(null);
  downloadEndTime = signal<number | null>(null);
  downloadDuration = computed(() => {
    const start = this.downloadStartTime();
    const end = this.downloadEndTime();
    return start && end ? end - start : null;
  });
  
  // Precache stats
  precacheStartTime = signal<number | null>(null);
  precacheEndTime = signal<number | null>(null);
  precacheDuration = computed(() => {
    const start = this.precacheStartTime();
    const end = this.precacheEndTime();
    return start && end ? end - start : null;
  });
  cacheHits = signal(0);
  cacheMisses = signal(0);
  
  // Cancel loading
  private currentAbortController: AbortController | null = null;
  private precacheAbortController: AbortController | null = null;
  
  // Cache TTL (5 minutes)
  private readonly CACHE_TTL = 5 * 60 * 1000;
  
  // Computed signals
  users = computed(() => this.paginationResponse()?.data ?? []);
  metadata = computed(() => this.paginationResponse()?.metadata ?? null);
  hasData = computed(() => this.users().length > 0);
  sortDirection = computed(() => this.direction().toLowerCase() as 'asc' | 'desc');
  sortActive = computed(() => UserListUtils.getSortActiveField(this.orderBy()));
  paginationConfig = computed((): PaginationConfig => ({
    size: this.size(),
    orderBy: this.orderBy(),
    direction: this.direction(),
    skipCount: skipCountCalculation(),
    orderByOptions: USER_LIST_CONFIG.sortOptions
  }));
  
  cachedPageCount = computed(() => this.pageCache().size);
  cacheHitRate = computed(() => {
    const hits = this.cacheHits();
    const misses = this.cacheMisses();
    const total = hits + misses;
    return total > 0 ? Math.round((hits / total) * 100) : 0;
  });

  constructor() {
    // Effect to trigger precaching when page/size changes
    effect(() => {
      const currentPage = this.page();
      const currentSize = this.size();
      const currentOrderBy = this.orderBy();
      const currentDirection = this.direction();
      
      // Only precache if we have data and we're not loading
      if (!this.loading() && this.hasData()) {
        this.precacheNextPage();
      }
    });
  }

  ngOnInit() {
    this.route.params.subscribe(params => {
      if (params['companyId']) {
        const id = +params['companyId'];
        this.companyId.set(id);
        this.title.set(`Users for Company ${id} (With Precaching)`);
      } else {
        this.companyId.set(null);
        this.title.set('Users (With Precaching)');
      }
      this.clearCache();
      this.loadUsers();
    });
  }

  private generateCacheKey(request: OffsetPaginationRequest, companyId: number | null): string {
    return `${companyId || 'all'}-${request.page}-${request.size}-${request.orderBy}-${request.direction}-${request.skipCount}`;
  }

  private isCacheValid(cachedPage: CachedPage): boolean {
    return Date.now() - cachedPage.timestamp < this.CACHE_TTL;
  }

  private clearCache() {
    this.pageCache.set(new Map());
    this.cacheHits.set(0);
    this.cacheMisses.set(0);
  }

  loadUsers() {
    // Cancel any ongoing requests
    if (this.currentAbortController) {
      this.currentAbortController.abort();
    }
    if (this.precacheAbortController) {
      this.precacheAbortController.abort();
    }
    
    const request: OffsetPaginationRequest = {
      page: this.page(),
      size: this.size(),
      orderBy: this.orderBy(),
      direction: this.direction(),
      skipCount: skipCountCalculation()
    };
    
    const cacheKey = this.generateCacheKey(request, this.companyId());
    const cachedPage = this.pageCache().get(cacheKey);
    
    // Check if we have a valid cached response
    if (cachedPage && this.isCacheValid(cachedPage)) {
      this.cacheHits.update(hits => hits + 1);
      this.paginationResponse.set(cachedPage.response);
      this.loading.set(false);
      this.error.set('');
      this.downloadStartTime.set(null);
      this.downloadEndTime.set(null);
      
      // Trigger precaching after cache hit as well
      setTimeout(() => this.precacheNextPage(), 0);
      return;
    }
    
    this.cacheMisses.update(misses => misses + 1);
    
    // Only set loading to true if we need to make a network request
    this.currentAbortController = new AbortController();
    this.loading.set(true);
    this.error.set('');
    this.downloadStartTime.set(performance.now());
    this.downloadEndTime.set(null);
    
    const options: RequestOptions = {
      abortController: this.currentAbortController
    };
    
    const companyId = this.companyId();
    const observable = companyId
      ? this.userService.getUsersByCompanyWithPagination(companyId, request, options)
      : this.userService.getUsersWithPagination(request, options);
    
    observable.subscribe({
      next: (response) => {
        this.downloadEndTime.set(performance.now());
        this.paginationResponse.set(response);
        
        // Cache the response
        const newCache = new Map(this.pageCache());
        newCache.set(cacheKey, {
          request,
          response,
          timestamp: Date.now()
        });
        this.pageCache.set(newCache);
        
        this.loading.set(false);
        this.currentAbortController = null;
        
        // Trigger precaching after successful load
        setTimeout(() => this.precacheNextPage(), 0);
      },
      error: (err) => {
        if (err.name === 'AbortError') {
          return;
        }
        this.downloadEndTime.set(performance.now());
        this.error.set(err.message || 'An error occurred');
        this.loading.set(false);
        this.currentAbortController = null;
      }
    });
  }

  private precacheNextPage() {
    const metadata = this.metadata();
    if (!metadata) {
      return;
    }
    
    // When count is skipped, we don't know if there's a next page until we try
    // So we allow precaching unless we explicitly know there's no next page
    const explicitlyNoNext = metadata.hasNextPage === false || 
                            (metadata.totalPages !== null && this.page() >= metadata.totalPages);
    
    if (explicitlyNoNext) {
      return; // No next page to precache
    }
    
    const nextPageRequest: OffsetPaginationRequest = {
      page: this.page() + 1,
      size: this.size(),
      orderBy: this.orderBy(),
      direction: this.direction(),
      skipCount: skipCountCalculation()
    };
    
    const cacheKey = this.generateCacheKey(nextPageRequest, this.companyId());
    const cachedPage = this.pageCache().get(cacheKey);
    
    if (cachedPage && this.isCacheValid(cachedPage)) {
      return;
    }
    
    // Cancel any existing precache request
    if (this.precacheAbortController) {
      this.precacheAbortController.abort();
    }
    
    this.precacheAbortController = new AbortController();
    this.precacheLoading.set(true);
    this.precacheStartTime.set(performance.now());
    this.precacheEndTime.set(null);
    
    const options: RequestOptions = {
      abortController: this.precacheAbortController
    };
    
    const companyId = this.companyId();
    const observable = companyId
      ? this.userService.getUsersByCompanyWithPagination(companyId, nextPageRequest, options)
      : this.userService.getUsersWithPagination(nextPageRequest, options);
    
    
    observable.subscribe({
      next: (response) => {
        this.precacheEndTime.set(performance.now());
        
        // Cache the precached response (even if empty - it's still valid)
        const newCache = new Map(this.pageCache());
        newCache.set(cacheKey, {
          request: nextPageRequest,
          response,
          timestamp: Date.now()
        });
        this.pageCache.set(newCache);
        
        this.precacheLoading.set(false);
        this.precacheAbortController = null;
      },
      error: (err) => {
        if (err.name === 'AbortError') {
          return;
        }
        this.precacheEndTime.set(performance.now());
        this.precacheLoading.set(false);
        this.precacheAbortController = null;
      }
    });
  }

  goToPage(page: number) {
    this.page.set(page);
    this.loadUsers();
  }

  onPageSizeChange() {
    this.page.set(1);
    this.clearCache(); // Clear cache when page size changes
    this.loadUsers();
  }
  
  updateOrderBy(value: string) {
    this.orderBy.set(value);
    this.clearCache(); // Clear cache when sort changes
    this.loadUsers();
  }
  
  updateDirection(value: 'ASC' | 'DESC') {
    this.direction.set(value);
    this.clearCache(); // Clear cache when sort changes
    this.loadUsers();
  }
  
  onSortChange(sort: Sort) {
    if (sort.active) {
      const newField = UserListUtils.getApiFieldName(sort.active);
      
      if (sort.direction) {
        // Normal case: set the field and direction
        this.orderBy.set(newField);
        this.direction.set(sort.direction.toUpperCase() as 'ASC' | 'DESC');
      } else {
        // When direction is empty (third click), flip to opposite direction
        const currentDirection = this.direction();
        this.orderBy.set(newField);
        this.direction.set(currentDirection === 'ASC' ? 'DESC' : 'ASC');
      }
    }
    this.clearCache(); // Clear cache when sort changes
    this.loadUsers();
  }
  
  updateSize(value: number) {
    this.size.set(value);
    this.onPageSizeChange();
  }
  
  cancelLoading() {
    if (this.currentAbortController) {
      this.currentAbortController.abort();
      this.currentAbortController = null;
      this.loading.set(false);
    }
    if (this.precacheAbortController) {
      this.precacheAbortController.abort();
      this.precacheAbortController = null;
      this.precacheLoading.set(false);
    }
  }
  
  clearCacheManually() {
    this.clearCache();
  }
}