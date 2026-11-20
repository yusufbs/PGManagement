import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { TenantsService } from '../../../core/services/tenants.service';
import { Tenant } from '../../../core/models/tenant.model';

@Component({
  selector: 'app-tenant-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule
  ],
  template: `
    <div class="tenants-container">
      <div class="header-row">
        <div>
          <h1>Tenants Directory</h1>
          <p class="subtitle">Manage PG residents, contact info, and room allocations</p>
        </div>
        <a mat-raised-button color="primary" routerLink="/tenants/new">
          <mat-icon>person_add</mat-icon> Onboard Tenant
        </a>
      </div>

      <div *ngIf="loading()" class="loading">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      </div>

      <div class="table-card mat-elevation-z2" *ngIf="!loading()">
        <table mat-table [dataSource]="tenants()" class="tenants-table">
          <!-- Name Column -->
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let t">
              <strong>{{ t.firstName }} {{ t.lastName }}</strong>
            </td>
          </ng-container>

          <!-- Contact Column -->
          <ng-container matColumnDef="contact">
            <th mat-header-cell *matHeaderCellDef>Contact Info</th>
            <td mat-cell *matCellDef="let t">
              <div>{{ t.email }}</div>
              <div class="phone">{{ t.phone }}</div>
            </td>
          </ng-container>

          <!-- Current Room Column -->
          <ng-container matColumnDef="room">
            <th mat-header-cell *matHeaderCellDef>Assigned Room</th>
            <td mat-cell *matCellDef="let t">
              <span *ngIf="t.currentRoomNumber" class="room-chip">Room {{ t.currentRoomNumber }}</span>
              <span *ngIf="!t.currentRoomNumber" class="unallocated">Unallocated</span>
            </td>
          </ng-container>

          <!-- ID Proof Column -->
          <ng-container matColumnDef="idProof">
            <th mat-header-cell *matHeaderCellDef>ID Proof</th>
            <td mat-cell *matCellDef="let t">
              {{ t.idProofType }}: {{ t.idProofNumber }}
            </td>
          </ng-container>

          <!-- Status Column -->
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let t">
              <span class="status-pill" [class.active]="t.isActive" [class.inactive]="!t.isActive">
                {{ t.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
          </ng-container>

          <!-- Actions Column -->
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef align="end">Actions</th>
            <td mat-cell *matCellDef="let t" align="end">
              <button mat-icon-button [routerLink]="['/tenants', t.tenantId, 'edit']" title="Edit">
                <mat-icon>edit</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
        </table>

        <mat-paginator
          [length]="totalCount()"
          [pageSize]="pageSize()"
          [pageSizeOptions]="[10, 20, 50]"
          (page)="onPageChange($event)">
        </mat-paginator>
      </div>
    </div>
  `,
  styles: [`
    .tenants-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    .header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      h1 { margin: 0; font-size: 26px; }
      .subtitle { margin: 4px 0 0; color: #666; }
    }
    .table-card {
      background: white;
      border-radius: 12px;
      overflow: hidden;
    }
    .tenants-table {
      width: 100%;
    }
    .phone {
      font-size: 12px;
      color: #666;
    }
    .room-chip {
      background: #e3f2fd;
      color: #1976d2;
      padding: 4px 8px;
      border-radius: 6px;
      font-weight: 500;
      font-size: 13px;
    }
    .unallocated {
      color: #999;
      font-style: italic;
    }
    .status-pill {
      display: inline-block;
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
      &.active { background: #e8f5e9; color: #2e7d32; }
      &.inactive { background: #ffebee; color: #c62828; }
    }
  `]
})
export class TenantListComponent implements OnInit {
  private tenantsService = inject(TenantsService);

  tenants = signal<Tenant[]>([]);
  totalCount = signal<number>(0);
  page = signal<number>(1);
  pageSize = signal<number>(20);
  loading = signal<boolean>(true);

  displayedColumns = ['name', 'contact', 'room', 'idProof', 'status', 'actions'];

  ngOnInit(): void {
    this.loadTenants();
  }

  loadTenants(): void {
    this.loading.set(true);
    this.tenantsService.getTenants(this.page(), this.pageSize()).subscribe({
      next: (res) => {
        this.tenants.set(res.data ?? []);
        this.totalCount.set(res.totalCount ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadTenants();
  }
}
