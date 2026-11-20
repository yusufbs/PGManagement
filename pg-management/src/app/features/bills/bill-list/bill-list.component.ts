import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BillsService } from '../../../core/services/bills.service';
import { Bill } from '../../../core/models/bill.model';
import { CurrencyInrPipe } from '../../../shared/pipes/currency-inr.pipe';

@Component({
  selector: 'app-bill-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatChipsModule,
    CurrencyInrPipe
  ],
  template: `
    <div class="bills-container">
      <div class="header-row">
        <div>
          <h1>Invoices & Billing History</h1>
          <p class="subtitle">Track generated invoices, payments received, and print receipts</p>
        </div>
        <div class="actions">
          <a mat-stroked-button routerLink="/bills/pending-dues" color="warn">
            <mat-icon>warning</mat-icon> View Pending Dues
          </a>
          <a mat-raised-button routerLink="/bills/generate" color="primary">
            <mat-icon>post_add</mat-icon> Generate Bills
          </a>
        </div>
      </div>

      <div *ngIf="loading()" class="loading">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      </div>

      <div class="table-card mat-elevation-z2" *ngIf="!loading()">
        <table mat-table [dataSource]="bills()" class="bills-table">
          <!-- Bill Id Column -->
          <ng-container matColumnDef="invoiceNo">
            <th mat-header-cell *matHeaderCellDef>Invoice #</th>
            <td mat-cell *matCellDef="let b">
              <strong>INV-{{ b.billYear }}-{{ b.billMonth }}-{{ b.billId }}</strong>
            </td>
          </ng-container>

          <!-- Tenant Column -->
          <ng-container matColumnDef="tenant">
            <th mat-header-cell *matHeaderCellDef>Tenant & Room</th>
            <td mat-cell *matCellDef="let b">
              <div><strong>{{ b.tenantName }}</strong></div>
              <div class="room-sub">Room {{ b.roomNumber }}</div>
            </td>
          </ng-container>

          <!-- Period Column -->
          <ng-container matColumnDef="period">
            <th mat-header-cell *matHeaderCellDef>Period</th>
            <td mat-cell *matCellDef="let b">
              {{ b.billMonth }}/{{ b.billYear }}
            </td>
          </ng-container>

          <!-- Total Column -->
          <ng-container matColumnDef="total">
            <th mat-header-cell *matHeaderCellDef>Total Amount</th>
            <td mat-cell *matCellDef="let b">
              <strong>{{ b.totalAmount | currencyInr }}</strong>
            </td>
          </ng-container>

          <!-- Paid Column -->
          <ng-container matColumnDef="paid">
            <th mat-header-cell *matHeaderCellDef>Paid</th>
            <td mat-cell *matCellDef="let b">
              <span class="paid-val">{{ b.paidAmount | currencyInr }}</span>
            </td>
          </ng-container>

          <!-- Balance Column -->
          <ng-container matColumnDef="balance">
            <th mat-header-cell *matHeaderCellDef>Balance Due</th>
            <td mat-cell *matCellDef="let b">
              <span [class.balance-due]="b.balanceDue > 0">{{ b.balanceDue | currencyInr }}</span>
            </td>
          </ng-container>

          <!-- Status Column -->
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let b">
              <span class="status-pill" [class.paid]="b.isPaid" [class.unpaid]="!b.isPaid">
                {{ b.isPaid ? 'PAID' : 'UNPAID' }}
              </span>
            </td>
          </ng-container>

          <!-- Print Action Column -->
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef align="end">Print</th>
            <td mat-cell *matCellDef="let b" align="end">
              <button mat-icon-button color="primary" (click)="onPrint(b)" title="Print PDF Bill">
                <mat-icon>print</mat-icon>
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
    .bills-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    .header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 24px;
      flex-wrap: wrap;
      gap: 16px;
      h1 { margin: 0; font-size: 26px; }
      .subtitle { margin: 4px 0 0; color: #666; }
    }
    .actions {
      display: flex;
      gap: 12px;
    }
    .table-card {
      background: white;
      border-radius: 12px;
      overflow: hidden;
    }
    .bills-table {
      width: 100%;
    }
    .room-sub {
      font-size: 12px;
      color: #666;
    }
    .paid-val { color: #2e7d32; font-weight: 500; }
    .balance-due { color: #d32f2f; font-weight: 700; }
    .status-pill {
      display: inline-block;
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 700;
      &.paid { background: #e8f5e9; color: #2e7d32; }
      &.unpaid { background: #ffebee; color: #d32f2f; }
    }
  `]
})
export class BillListComponent implements OnInit {
  private billsService = inject(BillsService);
  private snackBar = inject(MatSnackBar);

  bills = signal<Bill[]>([]);
  totalCount = signal<number>(0);
  page = signal<number>(1);
  pageSize = signal<number>(20);
  loading = signal<boolean>(true);

  displayedColumns = ['invoiceNo', 'tenant', 'period', 'total', 'paid', 'balance', 'status', 'actions'];

  ngOnInit(): void {
    this.loadBills();
  }

  loadBills(): void {
    this.loading.set(true);
    this.billsService.getBills({ page: this.page(), pageSize: this.pageSize() }).subscribe({
      next: (res) => {
        this.bills.set(res.data ?? []);
        this.totalCount.set(res.totalCount ?? 0);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: PageEvent): void {
    this.page.set(event.pageIndex + 1);
    this.pageSize.set(event.pageSize);
    this.loadBills();
  }

  onPrint(bill: Bill): void {
    this.snackBar.open('Downloading invoice PDF...', 'Close', { duration: 2000 });

    this.billsService.printBill(bill.billId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Invoice-${bill.billYear}-${bill.billMonth}-${bill.billId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.snackBar.open('Failed to generate PDF bill.', 'Close', { duration: 3000 });
      }
    });
  }
}
