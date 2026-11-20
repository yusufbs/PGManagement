import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BillsService } from '../../../core/services/bills.service';
import { PendingDue } from '../../../core/models/bill.model';
import { CurrencyInrPipe } from '../../../shared/pipes/currency-inr.pipe';
import { RecordPaymentDialogComponent } from '../../../shared/components/record-payment-dialog/record-payment-dialog.component';

@Component({
  selector: 'app-pending-dues',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatDialogModule,
    CurrencyInrPipe
  ],
  template: `
    <div class="dues-container">
      <div class="header-row">
        <div>
          <a mat-button routerLink="/bills" class="back-link">
            <mat-icon>arrow_back</mat-icon> Back to Invoices
          </a>
          <h1>Pending Dues & Overdue Tracker</h1>
          <p class="subtitle">Quickly identify tenants with outstanding rent and record cash/UPI payments</p>
        </div>
      </div>

      <div *ngIf="loading()" class="loading">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      </div>

      <div *ngIf="!loading() && dues().length === 0" class="empty-state">
        <mat-icon class="check-icon">verified</mat-icon>
        <h2>Zero Pending Dues!</h2>
        <p>All tenant balances are fully settled.</p>
      </div>

      <div class="table-card mat-elevation-z2" *ngIf="!loading() && dues().length > 0">
        <table mat-table [dataSource]="dues()" class="dues-table">
          <!-- Tenant Name Column -->
          <ng-container matColumnDef="tenant">
            <th mat-header-cell *matHeaderCellDef>Tenant Name</th>
            <td mat-cell *matCellDef="let d">
              <strong>{{ d.tenantName }}</strong>
            </td>
          </ng-container>

          <!-- Room Column -->
          <ng-container matColumnDef="room">
            <th mat-header-cell *matHeaderCellDef>Room</th>
            <td mat-cell *matCellDef="let d">
              <span class="room-chip">Room {{ d.roomNumber }}</span>
            </td>
          </ng-container>

          <!-- Overdue Months Column -->
          <ng-container matColumnDef="overdueMonths">
            <th mat-header-cell *matHeaderCellDef>Unpaid Invoices</th>
            <td mat-cell *matCellDef="let d">
              <span class="months-badge">{{ d.monthsOverdue }} month(s)</span>
            </td>
          </ng-container>

          <!-- Total Due Column -->
          <ng-container matColumnDef="totalDue">
            <th mat-header-cell *matHeaderCellDef>Outstanding Balance</th>
            <td mat-cell *matCellDef="let d">
              <span class="due-amount">{{ d.totalDue | currencyInr }}</span>
            </td>
          </ng-container>

          <!-- Last Payment Column -->
          <ng-container matColumnDef="lastPayment">
            <th mat-header-cell *matHeaderCellDef>Last Payment</th>
            <td mat-cell *matCellDef="let d">
              {{ d.lastPaymentDate ? (d.lastPaymentDate | date:'mediumDate') : 'No record' }}
            </td>
          </ng-container>

          <!-- Action Column -->
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef align="end">Actions</th>
            <td mat-cell *matCellDef="let d" align="end">
              <button mat-raised-button color="primary" (click)="onRecordPayment(d)">
                <mat-icon>payments</mat-icon> Record Payment
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .dues-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    .header-row {
      margin-bottom: 24px;
      .back-link { margin-bottom: 8px; }
      h1 { margin: 0; font-size: 26px; }
      .subtitle { margin: 4px 0 0; color: #666; }
    }
    .table-card {
      background: white;
      border-radius: 12px;
      overflow: hidden;
    }
    .dues-table {
      width: 100%;
    }
    .room-chip {
      background: #e3f2fd;
      color: #1976d2;
      padding: 4px 8px;
      border-radius: 6px;
      font-weight: 500;
    }
    .months-badge {
      background: #fff3e0;
      color: #e65100;
      padding: 3px 8px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
    }
    .due-amount {
      color: #d32f2f;
      font-weight: 700;
      font-size: 15px;
    }
    .empty-state {
      text-align: center;
      padding: 60px 20px;
      .check-icon { font-size: 64px; height: 64px; width: 64px; color: #2e7d32; }
      h2 { color: #2e7d32; margin-top: 16px; }
      p { color: #666; }
    }
  `]
})
export class PendingDuesComponent implements OnInit {
  private billsService = inject(BillsService);
  private dialog = inject(MatDialog);

  dues = signal<PendingDue[]>([]);
  loading = signal<boolean>(true);

  displayedColumns = ['tenant', 'room', 'overdueMonths', 'totalDue', 'lastPayment', 'actions'];

  ngOnInit(): void {
    this.loadDues();
  }

  loadDues(): void {
    this.loading.set(true);
    this.billsService.getPendingDues().subscribe({
      next: (res) => {
        this.dues.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onRecordPayment(due: PendingDue): void {
    const dialogRef = this.dialog.open(RecordPaymentDialogComponent, {
      data: {
        billId: 1, // Will apply against the tenant's oldest pending bill
        tenantName: due.tenantName,
        totalDue: due.totalDue
      }
    });

    dialogRef.afterClosed().subscribe((recorded) => {
      if (recorded) {
        this.loadDues();
      }
    });
  }
}
