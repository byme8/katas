import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CompanyService, RequestOptions } from '../../services/company.service';
import { Company } from '../../models/company.model';
import { OffsetPaginationRequest, OffsetPaginationResponse } from '../../models/pagination.model';
import { OffsetPaginationComponent, PaginationConfig } from '../offset-pagination/offset-pagination.component';
import { skipCountCalculation } from '../../stores/settings.store';

@Component({
  selector: 'app-company-list',
  standalone: true,
  imports: [CommonModule, RouterLink, OffsetPaginationComponent, MatTableModule, MatSortModule, MatCardModule, MatButtonModule, MatProgressSpinnerModule, MatIconModule, MatFormFieldModule, MatSelectModule, MatMenuModule, MatTooltipModule],
  templateUrl: './company-list.component.html',
  styleUrl: './company-list.component.scss'
})
export class CompanyListComponent implements OnInit {
  private readonly companyService = inject(CompanyService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  
  Math = Math;
  displayedColumns: string[] = ['id', 'name', 'email', 'createdAt', 'actions'];
  
  // State signals
  paginationResponse = signal<OffsetPaginationResponse<Company> | null>(null);
  loading = signal(true);
  error = signal('');
  page = signal(1);
  size = signal(25);
  orderBy = signal('CREATED_AT');
  direction = signal<'ASC' | 'DESC'>('DESC');
  
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
  companies = computed(() => this.paginationResponse()?.data ?? []);
  metadata = computed(() => this.paginationResponse()?.metadata ?? null);
  hasData = computed(() => this.companies().length > 0);
  sortDirection = computed(() => this.direction().toLowerCase() as 'asc' | 'desc');
  sortActive = computed(() => {
    const order = this.orderBy();
    return order === 'CREATED_AT' ? 'createdAt' : order.toLowerCase();
  });
  paginationConfig = computed((): PaginationConfig => ({
    size: this.size(),
    orderBy: this.orderBy(),
    direction: this.direction(),
    skipCount: skipCountCalculation(),
    orderByOptions: [
      { value: 'ID', label: 'ID' },
      { value: 'NAME', label: 'Name' },
      { value: 'EMAIL', label: 'Email' },
      { value: 'CREATED_AT', label: 'Created At' }
    ]
  }));

  ngOnInit() {
    this.route.queryParams.subscribe(queryParams => {
      // Set page from query params or default to 1
      const page = queryParams['page'] ? +queryParams['page'] : 1;
      this.page.set(page);
      
      // Set size from query params or default to 25
      const size = queryParams['size'] ? +queryParams['size'] : 25;
      this.size.set(size);
      
      // Set orderBy from query params or default to 'CREATED_AT'
      const orderBy = queryParams['orderBy'] || 'CREATED_AT';
      this.orderBy.set(orderBy);
      
      // Set direction from query params or default to 'DESC'
      const direction = queryParams['direction'] || 'DESC';
      this.direction.set(direction as 'ASC' | 'DESC');
      
      this.loadCompanies();
    });
  }

  loadCompanies() {
    // Cancel any ongoing request
    if (this.currentAbortController) {
      this.currentAbortController.abort();
    }
    
    this.currentAbortController = new AbortController();
    this.loading.set(true);
    this.error.set('');
    this.downloadStartTime.set(performance.now());
    this.downloadEndTime.set(null);
    
    const request: OffsetPaginationRequest = {
      page: this.page(),
      size: this.size(),
      orderBy: this.orderBy(),
      direction: this.direction(),
      skipCount: skipCountCalculation()
    };
    
    const options: RequestOptions = {
      abortController: this.currentAbortController
    };
    
    this.companyService.getCompaniesWithPagination(request, options).subscribe({
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

  goToPage(page: number) {
    this.updateUrl({ page });
  }

  onPageSizeChange() {
    this.updateUrl({ page: 1, size: this.size() });
  }
  
  updateOrderBy(value: string) {
    this.updateUrl({ orderBy: value });
  }
  
  updateDirection(value: 'ASC' | 'DESC') {
    this.updateUrl({ direction: value });
  }
  
  onSortChange(sort: Sort) {
    if (sort.active) {
      // Map column names to API field names
      const fieldMap: Record<string, string> = {
        'id': 'ID',
        'name': 'NAME',
        'email': 'EMAIL',
        'createdAt': 'CREATED_AT'
      };
      
      const newField = fieldMap[sort.active] || 'CREATED_AT';
      
      if (sort.direction) {
        // Normal case: set the field and direction
        this.updateUrl({ orderBy: newField, direction: sort.direction.toUpperCase() as 'ASC' | 'DESC' });
      } else {
        // When direction is empty (third click), flip to opposite direction
        const currentDirection = this.direction();
        this.updateUrl({ orderBy: newField, direction: currentDirection === 'ASC' ? 'DESC' : 'ASC' });
      }
    }
  }
  
  updateSize(value: number) {
    this.updateUrl({ page: 1, size: value });
  }
  
  cancelLoading() {
    if (this.currentAbortController) {
      this.currentAbortController.abort();
      this.currentAbortController = null;
      this.loading.set(false);
    }
  }

  private updateUrl(params: { page?: number; size?: number; orderBy?: string; direction?: 'ASC' | 'DESC' }) {
    const currentParams = { ...this.route.snapshot.queryParams };
    
    // Update parameters
    if (params.page !== undefined) currentParams['page'] = params.page.toString();
    if (params.size !== undefined) currentParams['size'] = params.size.toString();
    if (params.orderBy !== undefined) currentParams['orderBy'] = params.orderBy;
    if (params.direction !== undefined) currentParams['direction'] = params.direction;
    
    // Remove default values to keep URL clean
    if (currentParams['page'] === '1') delete currentParams['page'];
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