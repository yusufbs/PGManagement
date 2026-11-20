import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { PaymentsService } from '../../../core/services/payments.service';
import { MatSnackBar } from '@angular/material/snack-bar';

export interface RecordPaymentDialogData {
  billId: number;
  tenantName: string;
  totalDue: number;
}

@Component({
  selector: 'app-record-payment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  template: `
    <h2 mat-dialog-title>Record Payment for {{ data.tenantName }}</h2>
    <mat-dialog-content>
      <p>Outstanding Due: <strong>₹{{ data.totalDue }}</strong></p>
      <form [formGroup]="paymentForm" class="payment-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Amount Paid (₹)</mat-label>
          <input matInput type="number" formControlName="amountPaid" />
          <mat-error *ngIf="paymentForm.get('amountPaid')?.hasError('required')">Amount is required</mat-error>
          <mat-error *ngIf="paymentForm.get('amountPaid')?.hasError('min')">Amount must be greater than 0</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Payment Mode</mat-label>
          <mat-select formControlName="paymentMode">
            <mat-option value="UPI">UPI (GPay / PhonePe / Paytm)</mat-option>
            <mat-option value="Cash">Cash</mat-option>
            <mat-option value="BankTransfer">Bank Transfer (NEFT/IMPS)</mat-option>
            <mat-option value="Card">Card</mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Transaction Reference #</mat-label>
          <input matInput formControlName="transactionRef" placeholder="e.g. UPI Ref / Cheque No" />
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Remarks / Notes</mat-label>
          <textarea matInput formControlName="remarks" rows="2"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" [disabled]="paymentForm.invalid || isSubmitting" (click)="onSubmit()">
        {{ isSubmitting ? 'Recording...' : 'Record Payment' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .payment-form {
      display: flex;
      flex-direction: column;
      gap: 10px;
      min-width: 380px;
      margin-top: 10px;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class RecordPaymentDialogComponent {
  private fb = inject(FormBuilder);
  private paymentsService = inject(PaymentsService);
  private snackBar = inject(MatSnackBar);

  paymentForm: FormGroup;
  isSubmitting = false;

  constructor(
    public dialogRef: MatDialogRef<RecordPaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RecordPaymentDialogData
  ) {
    this.paymentForm = this.fb.group({
      amountPaid: [data.totalDue, [Validators.required, Validators.min(1)]],
      paymentMode: ['UPI', Validators.required],
      transactionRef: [''],
      remarks: ['']
    });
  }

  onSubmit(): void {
    if (this.paymentForm.invalid) return;

    this.isSubmitting = true;
    const formVal = this.paymentForm.value;

    this.paymentsService.createPayment({
      billId: this.data.billId,
      amountPaid: formVal.amountPaid,
      paymentMode: formVal.paymentMode,
      transactionRef: formVal.transactionRef,
      remarks: formVal.remarks
    }).subscribe({
      next: () => {
        this.snackBar.open('Payment recorded successfully!', 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
