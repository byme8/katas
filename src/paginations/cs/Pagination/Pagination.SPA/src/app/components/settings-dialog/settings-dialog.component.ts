import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { skipCountCalculation, setSkipCountCalculation, resetSkipCountCalculation } from '../../stores/settings.store';

@Component({
  selector: 'app-settings-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSlideToggleModule,
    MatIconModule
  ],
  templateUrl: './settings-dialog.component.html',
  styleUrl: './settings-dialog.component.scss'
})
export class SettingsDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<SettingsDialogComponent>);

  // Local copy for dialog editing (not a signal for ngModel compatibility)
  localSkipCountCalculation = skipCountCalculation();

  onSave(): void {
    setSkipCountCalculation(this.localSkipCountCalculation);
    this.dialogRef.close();
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onReset(): void {
    resetSkipCountCalculation();
    this.localSkipCountCalculation = skipCountCalculation();
  }
}