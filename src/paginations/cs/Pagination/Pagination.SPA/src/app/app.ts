import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SettingsDialogComponent } from './components/settings-dialog/settings-dialog.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly dialog = inject(MatDialog);
  
  protected title = 'Pagination Demo';

  openSettings(): void {
    this.dialog.open(SettingsDialogComponent, {
      width: '500px',
      disableClose: false
    });
  }
}
