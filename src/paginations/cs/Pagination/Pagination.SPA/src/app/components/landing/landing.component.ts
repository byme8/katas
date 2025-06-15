import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent {
  constructor(private router: Router) {}

  navigateToSmallCompany() {
    this.router.navigate(['/demo/small-company']);
  }

  navigateToLargeCompanyWithoutPagination() {
    this.router.navigate(['/demo/large-company-no-pagination']);
  }

  navigateToLargeCompanyWithPagination() {
    this.router.navigate(['/demo/large-company-with-pagination']);
  }

  navigateToAllUsersWithPagination() {
    this.router.navigate(['/demo/all-users-with-pagination']);
  }

  navigateToAllUsersWithoutPagination() {
    this.router.navigate(['/demo/all-users-no-pagination']);
  }
}