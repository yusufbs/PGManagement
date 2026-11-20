import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TenantsService } from '../../../core/services/tenants.service';

@Component({
  selector: 'app-tenant-form',
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
        <a mat-button routerLink="/tenants" class="back-link">
          <mat-icon>arrow_back</mat-icon> Back to Tenants
        </a>
        <h1>{{ isEditMode ? 'Edit Tenant' : 'Onboard New Tenant' }}</h1>
        <p class="subtitle">Record identity proof, contact details, and emergency references</p>
      </div>

      <mat-card class="form-card">
        <mat-card-content>
          <form [formGroup]="tenantForm" (ngSubmit)="onSubmit()" class="form-grid">
            <mat-form-field appearance="outline" class="col-6">
              <mat-label>First Name</mat-label>
              <input matInput formControlName="firstName" />
              <mat-error *ngIf="tenantForm.get('firstName')?.hasError('required')">First name is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Last Name</mat-label>
              <input matInput formControlName="lastName" />
              <mat-error *ngIf="tenantForm.get('lastName')?.hasError('required')">Last name is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Email Address</mat-label>
              <input matInput type="email" formControlName="email" />
              <mat-error *ngIf="tenantForm.get('email')?.hasError('required')">Email is required</mat-error>
              <mat-error *ngIf="tenantForm.get('email')?.hasError('email')">Invalid email address</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Phone Number</mat-label>
              <input matInput formControlName="phone" placeholder="+91..." />
              <mat-error *ngIf="tenantForm.get('phone')?.hasError('required')">Phone is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Identity Proof Type</mat-label>
              <mat-select formControlName="idProofType">
                <mat-option value="Aadhar">Aadhar Card</mat-option>
                <mat-option value="Passport">Passport</mat-option>
                <mat-option value="DL">Driving License</mat-option>
                <mat-option value="VoterId">Voter ID</mat-option>
              </mat-select>
              <mat-error *ngIf="tenantForm.get('idProofType')?.hasError('required')">Proof type is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>ID Proof Number</mat-label>
              <input matInput formControlName="idProofNumber" />
              <mat-error *ngIf="tenantForm.get('idProofNumber')?.hasError('required')">ID number is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-12">
              <mat-label>Emergency Contact (Name & Phone)</mat-label>
              <input matInput formControlName="emergencyContact" placeholder="e.g. Father: Ramesh Sharma +919876543210" />
            </mat-form-field>

            <div class="actions col-12">
              <a mat-button routerLink="/tenants">Cancel</a>
              <button mat-raised-button color="primary" type="submit" [disabled]="tenantForm.invalid || isSubmitting">
                {{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update Tenant' : 'Onboard Tenant') }}
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
export class TenantFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tenantsService = inject(TenantsService);
  private snackBar = inject(MatSnackBar);

  tenantForm!: FormGroup;
  isEditMode = false;
  tenantId?: number;
  isSubmitting = false;

  ngOnInit(): void {
    this.initForm();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.tenantId = parseInt(idParam, 10);
      this.loadTenant(this.tenantId);
    }
  }

  private initForm(): void {
    this.tenantForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      idProofType: ['Aadhar', Validators.required],
      idProofNumber: ['', Validators.required],
      emergencyContact: ['']
    });
  }

  private loadTenant(id: number): void {
    this.tenantsService.getTenantById(id).subscribe({
      next: (res) => {
        const t = res.data;
        if (t) {
          this.tenantForm.patchValue({
            firstName: t.firstName,
            lastName: t.lastName,
            email: t.email,
            phone: t.phone,
            idProofType: t.idProofType,
            idProofNumber: t.idProofNumber,
            emergencyContact: t.emergencyContact
          });
        }
      }
    });
  }

  onSubmit(): void {
    if (this.tenantForm.invalid) return;

    this.isSubmitting = true;
    const formVal = this.tenantForm.value;

    if (this.isEditMode && this.tenantId) {
      this.tenantsService.updateTenant(this.tenantId, formVal).subscribe({
        next: () => {
          this.snackBar.open('Tenant updated successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/tenants']);
        },
        error: () => this.isSubmitting = false
      });
    } else {
      this.tenantsService.createTenant(formVal).subscribe({
        next: () => {
          this.snackBar.open('Tenant onboarded successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/tenants']);
        },
        error: () => this.isSubmitting = false
      });
    }
  }
}
