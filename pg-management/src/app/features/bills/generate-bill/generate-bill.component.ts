import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BillsService } from '../../../core/services/bills.service';
import { AllocationsService } from '../../../core/services/allocations.service';
import { Allocation } from '../../../core/models/allocation.model';

@Component({
  selector: 'app-generate-bill',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="form-container">
      <div class="header">
        <a mat-button routerLink="/bills" class="back-link">
          <mat-icon>arrow_back</mat-icon> Back to Bills
        </a>
        <h1>Generate Monthly Tenant Bill</h1>
        <p class="subtitle">Calculate monthly rent, electricity units, water, and add discounts</p>
      </div>

      <mat-card class="form-card">
        <mat-card-content>
          <form [formGroup]="billForm" (ngSubmit)="onSubmit()" class="form-grid">
            <mat-form-field appearance="outline" class="col-12">
              <mat-label>Select Resident Allocation</mat-label>
              <mat-select formControlName="allocationId">
                <mat-option *ngFor="let a of allocations()" [value]="a.allocationId">
                  {{ a.tenantName }} (Room {{ a.roomNumber }})
                </mat-option>
              </mat-select>
              <mat-error *ngIf="billForm.get('allocationId')?.hasError('required')">Allocation selection is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Billing Month</mat-label>
              <mat-select formControlName="billMonth">
                <mat-option *ngFor="let m of months" [value]="m.val">{{ m.label }}</mat-option>
              </mat-select>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Billing Year</mat-label>
              <input matInput type="number" formControlName="billYear" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Electricity Charges (₹)</mat-label>
              <input matInput type="number" formControlName="electricityCharges" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Water Charges (₹)</mat-label>
              <input matInput type="number" formControlName="waterCharges" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Other Charges (₹)</mat-label>
              <input matInput type="number" formControlName="otherCharges" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Discount (₹)</mat-label>
              <input matInput type="number" formControlName="discount" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Payment Due Date</mat-label>
              <input matInput type="date" formControlName="dueDate" />
              <mat-error *ngIf="billForm.get('dueDate')?.hasError('required')">Due date is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Notes / Comments</mat-label>
              <input matInput formControlName="notes" placeholder="e.g. 50 units @ Rs. 9/unit" />
            </mat-form-field>

            <div class="actions col-12">
              <a mat-button routerLink="/bills">Cancel</a>
              <button mat-raised-button color="primary" type="submit" [disabled]="billForm.invalid || isSubmitting">
                {{ isSubmitting ? 'Generating...' : 'Generate Bill' }}
              </button>
            </div>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .form-container {
      padding: 24px;
      max-width: 800px;
      margin: 0 auto;
    }
    .header {
      margin-bottom: 24px;
      h1 { margin: 0; font-size: 26px; }
      .subtitle { margin: 4px 0 0; color: #666; }
    }
    .form-card {
      border-radius: 12px;
      padding: 24px;
    }
    .form-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 16px;
    }
    .col-6 {
      flex: 1 1 calc(50% - 16px);
      min-width: 250px;
    }
    .col-12 {
      width: 100%;
    }
    .actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      margin-top: 16px;
    }
  `]
})
export class GenerateBillComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private billsService = inject(BillsService);
  private allocationsService = inject(AllocationsService);
  private snackBar = inject(MatSnackBar);

  billForm!: FormGroup;
  allocations = signal<Allocation[]>([]);
  isSubmitting = false;

  months = [
    { val: 1, label: 'January' }, { val: 2, label: 'February' }, { val: 3, label: 'March' },
    { val: 4, label: 'April' }, { val: 5, label: 'May' }, { val: 6, label: 'June' },
    { val: 7, label: 'July' }, { val: 8, label: 'August' }, { val: 9, label: 'September' },
    { val: 10, label: 'October' }, { val: 11, label: 'November' }, { val: 12, label: 'December' }
  ];

  ngOnInit(): void {
    const today = new Date();
    const dueDate = new Date();
    dueDate.setDate(dueDate.getDate() + 7);

    this.billForm = this.fb.group({
      allocationId: [null, Validators.required],
      billMonth: [today.getMonth() + 1, Validators.required],
      billYear: [today.getFullYear(), Validators.required],
      electricityCharges: [0, [Validators.min(0)]],
      waterCharges: [0, [Validators.min(0)]],
      otherCharges: [0, [Validators.min(0)]],
      discount: [0, [Validators.min(0)]],
      dueDate: [dueDate.toISOString().substring(0, 10), Validators.required],
      notes: ['']
    });

    this.allocationsService.getAllocations().subscribe({
      next: (res) => this.allocations.set(res.data ?? [])
    });
  }

  onSubmit(): void {
    if (this.billForm.invalid) return;

    this.isSubmitting = true;
    const formVal = this.billForm.value;

    this.billsService.generateBill(formVal).subscribe({
      next: () => {
        this.snackBar.open('Monthly bill generated successfully!', 'Close', { duration: 3000 });
        this.router.navigate(['/bills']);
      },
      error: () => this.isSubmitting = false
    });
  }
}
