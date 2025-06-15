import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UserService, RequestOptions } from '../../services/user.service';
import { User } from '../../models/user.model';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { USER_LIST_CONFIG, UserListUtils } from '../shared/user-list-config';

@Component({
  selector: 'app-user-list-client',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatButtonModule,
    FormsModule
  ],
  template: `
    <mat-card>
      <mat-card-header>
        <mat-card-title>
          @if (companyId()) {
            Users for Company #{{companyId()}} (Client-Side Pagination)
          } @else {
            All Users (Client-Side Pagination)
          }
        </mat-card-title>
        @if (downloadDuration()) {
          <mat-card-subtitle class="download-stats">
            <mat-icon>schedule</mat-icon>
            Download time: {{ downloadDuration()! | number:'1.0-0' }}ms
          </mat-card-subtitle>
        }
      </mat-card-header>
      <mat-card-content>
        @if (loading()) {
          <div class="loading-container">
            <mat-spinner></mat-spinner>
            <p>Loading all users...</p>
            <button mat-button color="warn" (click)="cancelLoading()">
              <mat-icon>cancel</mat-icon>
              Cancel
            </button>
          </div>
        } @else if (error()) {
          <div class="error-container">
            <p>Error: {{ error() }}</p>
          </div>
        } @else {
          <div class="controls-container">
            <mat-form-field>
              <mat-label>Search</mat-label>
              <input matInput [(ngModel)]="searchTerm" (ngModelChange)="onSearchChange()" placeholder="Filter by name or email">
            </mat-form-field>
          </div>

          <div class="table-container">
            <table mat-table [dataSource]="paginatedUsers()" matSort (matSortChange)="onSortChange($event)">
              <ng-container matColumnDef="id">
                <th mat-header-cell *matHeaderCellDef mat-sort-header disabled>ID</th>
                <td mat-cell *matCellDef="let user">{{ user.id }}</td>
              </ng-container>

              <ng-container matColumnDef="companyId">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Company ID</th>
                <td mat-cell *matCellDef="let user">{{ user.companyId }}</td>
              </ng-container>

              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Name</th>
                <td mat-cell *matCellDef="let user">{{ user.name }}</td>
              </ng-container>

              <ng-container matColumnDef="email">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Email</th>
                <td mat-cell *matCellDef="let user">{{ user.email }}</td>
              </ng-container>

              <ng-container matColumnDef="phoneNumber">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Phone</th>
                <td mat-cell *matCellDef="let user">{{ user.phoneNumber || '-' }}</td>
              </ng-container>

              <ng-container matColumnDef="twitterHandle">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Twitter</th>
                <td mat-cell *matCellDef="let user">
                  <span *ngIf="user.twitterHandle" class="social-handle">{{ '@' + user.twitterHandle }}</span>
                  <span *ngIf="!user.twitterHandle">-</span>
                </td>
              </ng-container>

              <ng-container matColumnDef="facebookProfile">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Facebook</th>
                <td mat-cell *matCellDef="let user">{{ user.facebookProfile || '-' }}</td>
              </ng-container>

              <ng-container matColumnDef="whatsAppNumber">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>WhatsApp</th>
                <td mat-cell *matCellDef="let user">{{ user.whatsAppNumber || '-' }}</td>
              </ng-container>

              <ng-container matColumnDef="instagramHandle">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Instagram</th>
                <td mat-cell *matCellDef="let user">
                  <span *ngIf="user.instagramHandle" class="social-handle">{{ '@' + user.instagramHandle }}</span>
                  <span *ngIf="!user.instagramHandle">-</span>
                </td>
              </ng-container>

              <ng-container matColumnDef="blueskyHandle">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Bluesky</th>
                <td mat-cell *matCellDef="let user">
                  <span *ngIf="user.blueskyHandle" class="social-handle">{{ '@' + user.blueskyHandle }}</span>
                  <span *ngIf="!user.blueskyHandle">-</span>
                </td>
              </ng-container>

              <ng-container matColumnDef="createdAt">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Created At</th>
                <td mat-cell *matCellDef="let user">{{ user.createdAt | date:'medium' }}</td>
              </ng-container>

              <ng-container matColumnDef="updatedAt">
                <th mat-header-cell *matHeaderCellDef mat-sort-header>Updated At</th>
                <td mat-cell *matCellDef="let user">{{ user.updatedAt | date:'medium' }}</td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>
          </div>

          <div class="pagination-controls">
            <div class="pagination-info">
              Showing {{ startIndex() + 1 }} - {{ endIndex() }} of {{ filteredUsers().length }} users
              @if (searchTerm.trim()) {
                (filtered from {{ allUsers().length }} total)
              }
            </div>
            
            <div class="pagination-buttons">
              <button mat-button [disabled]="currentPage() === 0" (click)="goToPage(0)">First</button>
              <button mat-button [disabled]="currentPage() === 0" (click)="previousPage()">Previous</button>
              
              <span class="page-info">{{ currentPage() + 1 }} / {{ totalPages() }}</span>
              
              <button mat-button [disabled]="currentPage() === totalPages() - 1" (click)="nextPage()">Next</button>
              <button mat-button [disabled]="currentPage() === totalPages() - 1" (click)="goToPage(totalPages() - 1)">Last</button>
            </div>
            
            <mat-form-field class="page-size-select">
              <mat-label>Page Size</mat-label>
              <mat-select [(ngModel)]="pageSize" (ngModelChange)="onPageSizeChange()">
                <mat-option [value]="10">10</mat-option>
                <mat-option [value]="25">25</mat-option>
                <mat-option [value]="50">50</mat-option>
                <mat-option [value]="100">100</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    mat-card {
      height: calc(100vh - 150px);
      display: flex;
      flex-direction: column;
      
      mat-card-content {
        flex: 1;
        display: flex;
        flex-direction: column;
        overflow: hidden;
      }
    }

    .loading-container, .error-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 2rem;
      gap: 16px;
    }
    
    .loading-container button, .error-container button {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .loading-container p, .error-container p {
      margin: 0;
    }

    .controls-container {
      display: flex;
      gap: 1rem;
      margin-bottom: 1rem;
      flex-wrap: wrap;
    }

    .controls-container mat-form-field {
      width: 100%;
      max-width: 400px;
    }

    .table-container {
      flex: 1;
      overflow: auto;
      min-height: 0;
    }

    table {
      width: 100%;
    }

    .pagination-controls {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-top: 1rem;
      flex-wrap: wrap;
      gap: 1rem;
    }

    .pagination-buttons {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .page-info {
      margin: 0 0.5rem;
      font-weight: 500;
    }

    .page-size-select {
      width: 100px;
    }

    .pagination-info {
      font-size: 0.875rem;
      color: rgba(0, 0, 0, 0.6);
    }
    
    .download-stats {
      display: flex;
      align-items: center;
      gap: 4px;
      color: rgba(0, 0, 0, 0.6);
      font-size: 14px;
      
      mat-icon {
        font-size: 16px;
        width: 16px;
        height: 16px;
      }
    }
    
    .social-handle {
      color: #1976d2;
      font-family: monospace;
      font-size: 12px;
    }
  `]
})
export class UserListClientComponent {
  private route = inject(ActivatedRoute);
  private userService = inject(UserService);

  displayedColumns = USER_LIST_CONFIG.displayedColumns;
  userConfig = USER_LIST_CONFIG;
  
  companyId = signal<number | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  allUsers = signal<User[]>([]);
  
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
  
  searchTerm = '';
  sortField = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';
  pageSize = 25;
  currentPage = signal(0);

  filteredUsers = computed(() => {
    let users = this.allUsers();
    
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      users = users.filter(user => 
        user.name.toLowerCase().includes(term) || 
        user.email.toLowerCase().includes(term)
      );
    }
    
    const sortedUsers = [...users].sort((a, b) => {
      let aValue: any = a[this.sortField as keyof User];
      let bValue: any = b[this.sortField as keyof User];
      
      if (typeof aValue === 'string') {
        aValue = aValue.toLowerCase();
        bValue = bValue.toLowerCase();
      }
      
      if (this.sortDirection === 'asc') {
        return aValue > bValue ? 1 : -1;
      } else {
        return aValue < bValue ? 1 : -1;
      }
    });
    
    return sortedUsers;
  });

  totalPages = computed(() => 
    Math.ceil(this.filteredUsers().length / this.pageSize)
  );

  startIndex = computed(() => 
    this.currentPage() * this.pageSize
  );

  endIndex = computed(() => 
    Math.min(this.startIndex() + this.pageSize, this.filteredUsers().length)
  );

  paginatedUsers = computed(() => 
    this.filteredUsers().slice(this.startIndex(), this.endIndex())
  );

  constructor() {
    this.route.params.subscribe(params => {
      const companyId = params['companyId'];
      if (companyId) {
        this.companyId.set(Number(companyId));
      }
      this.loadAllUsers();
    });
  }

  private loadAllUsers() {
    // Cancel any ongoing request
    if (this.currentAbortController) {
      this.currentAbortController.abort();
    }
    
    this.currentAbortController = new AbortController();
    this.loading.set(true);
    this.error.set(null);
    this.downloadStartTime.set(performance.now());
    this.downloadEndTime.set(null);
    
    const options: RequestOptions = {
      abortController: this.currentAbortController
    };
    
    const companyId = this.companyId();
    const observable = companyId 
      ? this.userService.getUsersByCompany(companyId, options)
      : this.userService.getAllUsers(options);
    
    observable.subscribe({
      next: (users) => {
        this.downloadEndTime.set(performance.now());
        this.allUsers.set(users);
        this.loading.set(false);
        this.currentAbortController = null;
      },
      error: (err) => {
        if (err.name === 'AbortError') {
          // Request was cancelled, don't update state
          return;
        }
        this.downloadEndTime.set(performance.now());
        this.error.set(err.message || 'Failed to load users');
        this.loading.set(false);
        this.currentAbortController = null;
      }
    });
  }

  onSearchChange() {
    this.currentPage.set(0);
  }

  onSortChange(sort: Sort) {
    if (sort.active) {
      if (sort.direction) {
        // Normal case: set the field and direction
        this.sortField = sort.active;
        this.sortDirection = sort.direction as 'asc' | 'desc';
      } else {
        // When direction is empty (third click), flip to opposite direction
        this.sortField = sort.active;
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      }
    }
    this.currentPage.set(0);
  }

  onPageSizeChange() {
    this.currentPage.set(0);
  }

  previousPage() {
    if (this.currentPage() > 0) {
      this.currentPage.update(p => p - 1);
    }
  }

  nextPage() {
    if (this.currentPage() < this.totalPages() - 1) {
      this.currentPage.update(p => p + 1);
    }
  }

  goToPage(page: number) {
    const pageNum = Number(page);
    if (!isNaN(pageNum) && pageNum >= 0 && pageNum < this.totalPages()) {
      this.currentPage.set(pageNum);
    }
  }
  
  cancelLoading() {
    if (this.currentAbortController) {
      this.currentAbortController.abort();
      this.currentAbortController = null;
      this.loading.set(false);
    }
  }
}