import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { OffsetPaginationMetadata } from '../../models/pagination.model';

export interface PaginationConfig {
  size: number;
  orderBy: string;
  direction: 'ASC' | 'DESC';
  skipCount: boolean;
  orderByOptions: { value: string; label: string }[];
}

export interface PaginationEvents {
  sizeChange: number;
  orderByChange: string;
  directionChange: 'ASC' | 'DESC';
  pageChange: number;
}

@Component({
  selector: 'app-offset-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './offset-pagination.component.html',
  styleUrl: './offset-pagination.component.scss'
})
export class OffsetPaginationComponent {
  @Input() metadata: OffsetPaginationMetadata | null = null;
  @Input() config!: PaginationConfig;
  @Input() entityName: string = 'items';
  @Input() showControls: boolean = true;
  @Input() showInfo: boolean = true;

  @Output() sizeChange = new EventEmitter<number>();
  @Output() orderByChange = new EventEmitter<string>();
  @Output() directionChange = new EventEmitter<'ASC' | 'DESC'>();
  @Output() pageChange = new EventEmitter<number>();

  onSizeChange(value: number) {
    this.sizeChange.emit(value);
  }

  onOrderByChange(value: string) {
    this.orderByChange.emit(value);
  }

  onDirectionChange(value: 'ASC' | 'DESC') {
    this.directionChange.emit(value);
  }

  onPageChange(page: number) {
    this.pageChange.emit(page);
  }
}