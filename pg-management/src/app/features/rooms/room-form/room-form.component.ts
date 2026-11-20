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
import { RoomsService } from '../../../core/services/rooms.service';

@Component({
  selector: 'app-room-form',
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
        <a mat-button routerLink="/rooms" class="back-link">
          <mat-icon>arrow_back</mat-icon> Back to Rooms
        </a>
        <h1>{{ isEditMode ? 'Edit Room' : 'Register New Room' }}</h1>
        <p class="subtitle">Specify room attributes, pricing, and amenities</p>
      </div>

      <mat-card class="form-card">
        <mat-card-content>
          <form [formGroup]="roomForm" (ngSubmit)="onSubmit()" class="form-grid">
            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Room Number</mat-label>
              <input matInput formControlName="roomNumber" placeholder="e.g. 101, 202-A" />
              <mat-error *ngIf="roomForm.get('roomNumber')?.hasError('required')">Room number is required</mat-error>
              <mat-error *ngIf="roomForm.get('roomNumber')?.hasError('maxlength')">Max 20 characters</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Floor</mat-label>
              <input matInput type="number" formControlName="floor" placeholder="e.g. 1, 2" />
              <mat-error *ngIf="roomForm.get('floor')?.hasError('required')">Floor is required</mat-error>
              <mat-error *ngIf="roomForm.get('floor')?.hasError('min')">Must be 0 or higher</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Room Type</mat-label>
              <mat-select formControlName="roomType">
                <mat-option value="Single">Single (1 Bed)</mat-option>
                <mat-option value="Double">Double Sharing (2 Beds)</mat-option>
                <mat-option value="Triple">Triple Sharing (3 Beds)</mat-option>
                <mat-option value="Dormitory">Dormitory (4+ Beds)</mat-option>
              </mat-select>
              <mat-error *ngIf="roomForm.get('roomType')?.hasError('required')">Room type is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Total Bed Capacity</mat-label>
              <input matInput type="number" formControlName="capacity" placeholder="e.g. 2" />
              <mat-error *ngIf="roomForm.get('capacity')?.hasError('required')">Capacity is required</mat-error>
              <mat-error *ngIf="roomForm.get('capacity')?.hasError('min')">At least 1 bed</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Monthly Rent per Bed (₹)</mat-label>
              <input matInput type="number" formControlName="monthlyRent" placeholder="e.g. 6500" />
              <mat-error *ngIf="roomForm.get('monthlyRent')?.hasError('required')">Rent is required</mat-error>
              <mat-error *ngIf="roomForm.get('monthlyRent')?.hasError('min')">Rent must be greater than 0</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="col-6">
              <mat-label>Amenities</mat-label>
              <input matInput formControlName="amenities" placeholder="e.g. AC, Attached Bath, WiFi, Balcony" />
            </mat-form-field>

            <div class="actions col-12">
              <a mat-button routerLink="/rooms">Cancel</a>
              <button mat-raised-button color="primary" type="submit" [disabled]="roomForm.invalid || isSubmitting">
                {{ isSubmitting ? 'Saving...' : (isEditMode ? 'Update Room' : 'Register Room') }}
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
export class RoomFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private roomsService = inject(RoomsService);
  private snackBar = inject(MatSnackBar);

  roomForm!: FormGroup;
  isEditMode = false;
  roomId?: number;
  isSubmitting = false;

  ngOnInit(): void {
    this.initForm();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEditMode = true;
      this.roomId = parseInt(idParam, 10);
      this.loadRoom(this.roomId);
    }
  }

  private initForm(): void {
    this.roomForm = this.fb.group({
      roomNumber: ['', [Validators.required, Validators.maxLength(20)]],
      floor: [1, [Validators.required, Validators.min(0)]],
      roomType: ['Double', Validators.required],
      capacity: [2, [Validators.required, Validators.min(1), Validators.max(20)]],
      monthlyRent: [5000, [Validators.required, Validators.min(1)]],
      amenities: ['']
    });
  }

  private loadRoom(id: number): void {
    this.roomsService.getRoomById(id).subscribe({
      next: (res) => {
        const r = res.data;
        if (r) {
          this.roomForm.patchValue({
            roomNumber: r.roomNumber,
            floor: r.floor,
            roomType: r.roomType,
            capacity: r.capacity,
            monthlyRent: r.monthlyRent,
            amenities: r.amenities
          });
        }
      }
    });
  }

  onSubmit(): void {
    if (this.roomForm.invalid) return;

    this.isSubmitting = true;
    const formVal = this.roomForm.value;

    if (this.isEditMode && this.roomId) {
      this.roomsService.updateRoom(this.roomId, formVal).subscribe({
        next: () => {
          this.snackBar.open('Room updated successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/rooms']);
        },
        error: () => this.isSubmitting = false
      });
    } else {
      this.roomsService.createRoom(formVal).subscribe({
        next: () => {
          this.snackBar.open('Room registered successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/rooms']);
        },
        error: () => this.isSubmitting = false
      });
    }
  }
}
