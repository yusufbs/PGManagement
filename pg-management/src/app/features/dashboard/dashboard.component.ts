import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { RouterModule } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardStats } from '../../core/models/dashboard.model';
import { CurrencyInrPipe } from '../../shared/pipes/currency-inr.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressBarModule,
    MatIconModule,
    MatButtonModule,
    RouterModule,
    CurrencyInrPipe
  ],
  template: `
    <div class="dashboard-container">
      <div class="header">
        <h1>PG Management Overview</h1>
        <p class="subtitle">Real-time room occupancy, pending dues, and revenue tracking</p>
      </div>

      <div *ngIf="loading()" class="loading-state">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      </div>

      <div *ngIf="stats() as s" class="dashboard-content">
        <!-- 4 Summary KPI Cards -->
        <div class="kpi-grid">
          <mat-card class="kpi-card total-rooms">
            <mat-card-header>
              <mat-icon mat-card-avatar color="primary">meeting_room</mat-icon>
              <mat-card-title>Total Rooms</mat-card-title>
              <mat-card-subtitle>{{ s.totalRooms }} registered rooms</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="kpi-value">{{ s.totalRooms }}</div>
              <div class="kpi-subtext">Across all floors</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="kpi-card occupied-rooms">
            <mat-card-header>
              <mat-icon mat-card-avatar class="icon-occupied">person</mat-icon>
              <mat-card-title>Occupied Rooms</mat-card-title>
              <mat-card-subtitle>{{ s.totalActiveTenants }} active tenants</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="kpi-value">{{ s.occupiedRooms }}</div>
              <mat-progress-bar mode="determinate" [value]="s.occupancyPercent" color="primary"></mat-progress-bar>
              <div class="kpi-subtext">{{ s.occupancyPercent }}% occupancy rate</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="kpi-card available-rooms">
            <mat-card-header>
              <mat-icon mat-card-avatar class="icon-available">check_circle</mat-icon>
              <mat-card-title>Available Rooms</mat-card-title>
              <mat-card-subtitle>Ready to allocate</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="kpi-value">{{ s.availableRooms }}</div>
              <div class="kpi-subtext">
                <a routerLink="/rooms/available" class="link">View empty rooms &rarr;</a>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="kpi-card pending-dues" [class.has-dues]="s.totalPendingDues > 0">
            <mat-card-header>
              <mat-icon mat-card-avatar class="icon-dues">money_off</mat-icon>
              <mat-card-title>Pending Dues</mat-card-title>
              <mat-card-subtitle>{{ s.overdueBillsCount }} overdue bills</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="kpi-value dues-value">{{ s.totalPendingDues | currencyInr }}</div>
              <div class="kpi-subtext">
                <a routerLink="/bills/pending-dues" class="link">Review dues &rarr;</a>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- Secondary Section: Revenue & Quick Actions -->
        <div class="secondary-grid">
          <mat-card class="chart-card">
            <mat-card-header>
              <mat-icon mat-card-avatar color="primary">insights</mat-icon>
              <mat-card-title>Monthly Revenue Trend</mat-card-title>
              <mat-card-subtitle>Current Month: {{ s.monthlyRevenue | currencyInr }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div class="revenue-bars">
                <div *ngFor="let item of s.last12MonthsRevenue" class="bar-col">
                  <div class="bar-wrapper">
                    <div class="bar" [style.height.%]="getBarHeight(item.revenue)"></div>
                  </div>
                  <span class="bar-label">{{ item.monthLabel }}</span>
                  <span class="bar-amount">₹{{ (item.revenue / 1000) | number:'1.0-1' }}k</span>
                </div>
              </div>
            </mat-card-content>
          </mat-card>

          <mat-card class="actions-card">
            <mat-card-header>
              <mat-icon mat-card-avatar color="accent">bolt</mat-icon>
              <mat-card-title>Quick Operations</mat-card-title>
            </mat-card-header>
            <mat-card-content class="actions-list">
              <a mat-stroked-button routerLink="/rooms/new" class="action-btn">
                <mat-icon>add</mat-icon> Register New Room
              </a>
              <a mat-stroked-button routerLink="/tenants/new" class="action-btn">
                <mat-icon>person_add</mat-icon> Onboard Tenant
              </a>
              <a mat-stroked-button routerLink="/bills/generate" class="action-btn">
                <mat-icon>receipt</mat-icon> Generate Monthly Bills
              </a>
              <a mat-stroked-button routerLink="/bills/pending-dues" class="action-btn">
                <mat-icon>payment</mat-icon> Record Dues Payment
              </a>
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    .header {
      margin-bottom: 24px;
      h1 { margin: 0; font-size: 28px; font-weight: 600; }
      .subtitle { margin: 4px 0 0; color: #666; font-size: 14px; }
    }
    .kpi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
      gap: 20px;
      margin-bottom: 28px;
    }
    .kpi-card {
      border-radius: 12px;
      padding: 16px;
      .kpi-value {
        font-size: 32px;
        font-weight: 700;
        margin: 12px 0 8px;
      }
      .kpi-subtext {
        font-size: 13px;
        color: #777;
        margin-top: 6px;
      }
    }
    .icon-occupied { color: #1976d2; }
    .icon-available { color: #388e3c; }
    .icon-dues { color: #d32f2f; }
    .dues-value { color: #d32f2f; }
    .link { color: #1976d2; text-decoration: none; font-weight: 500; }
    .secondary-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: 20px;
      @media (max-width: 900px) {
        grid-template-columns: 1fr;
      }
    }
    .revenue-bars {
      display: flex;
      justify-content: space-between;
      align-items: flex-end;
      height: 200px;
      padding-top: 20px;
      gap: 8px;
    }
    .bar-col {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      height: 100%;
    }
    .bar-wrapper {
      flex: 1;
      width: 100%;
      display: flex;
      align-items: flex-end;
      justify-content: center;
    }
    .bar {
      width: 20px;
      background: linear-gradient(180deg, #1976d2 0%, #64b5f6 100%);
      border-radius: 4px 4px 0 0;
      min-height: 4px;
      transition: height 0.4s ease;
    }
    .bar-label {
      font-size: 10px;
      color: #666;
      margin-top: 6px;
    }
    .bar-amount {
      font-size: 9px;
      font-weight: bold;
      color: #333;
    }
    .actions-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
      margin-top: 16px;
      .action-btn {
        justify-content: flex-start;
        padding: 12px 16px;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  stats = signal<DashboardStats | null>(null);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    this.dashboardService.getDashboard().subscribe({
      next: (data) => {
        this.stats.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getBarHeight(revenue: number): number {
    const max = 50000;
    return Math.min(100, Math.max(5, (revenue / max) * 100));
  }
}
