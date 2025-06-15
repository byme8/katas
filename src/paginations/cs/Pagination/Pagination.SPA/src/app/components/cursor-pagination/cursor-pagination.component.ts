import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { CursorPaginationMetadata } from '../../models/pagination.model';

export interface CursorPaginationConfig {
  size: number;
}

@Component({
  selector: 'app-cursor-pagination',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './cursor-pagination.component.html',
  styleUrl: './cursor-pagination.component.scss'
})
export class CursorPaginationComponent {
  @Input() metadata: CursorPaginationMetadata | null = null;
  @Input() config!: CursorPaginationConfig;
  @Input() entityName: string = 'items';
  @Input() showControls: boolean = true;
  @Input() showInfo: boolean = true;

  @Output() sizeChange = new EventEmitter<number>();
  @Output() nextPage = new EventEmitter<void>();
  @Output() previousPage = new EventEmitter<void>();
  @Output() firstPage = new EventEmitter<void>();
  @Output() lastPage = new EventEmitter<void>();

  onSizeChange(value: number) {
    this.sizeChange.emit(value);
  }

  onNextPage() {
    this.nextPage.emit();
  }

  onPreviousPage() {
    this.previousPage.emit();
  }

  onFirstPage() {
    this.firstPage.emit();
  }

  onLastPage() {
    this.lastPage.emit();
  }
}