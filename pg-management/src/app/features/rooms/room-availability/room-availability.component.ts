import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RoomsService } from '../../../core/services/rooms.service';
import { RoomAvailability } from '../../../core/models/room.model';
import { CurrencyInrPipe } from '../../../shared/pipes/currency-inr.pipe';

@Component({
  selector: 'app-room-availability',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    CurrencyInrPipe
  ],
  template: `
    <div class="availability-container">
      <div class="header">
        <a mat-button routerLink="/rooms" class="back-link">
          <mat-icon>arrow_back</mat-icon> Back to All Rooms
        </a>
        <h1>Empty & Available Rooms</h1>
        <p class="subtitle">Quick vacancy lookup for walk-in tenants and bookings</p>
      </div>

      <div *ngIf="loading()" class="loading">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      </div>

      <div *ngIf="!loading() && rooms().length === 0" class="empty-state">
        <mat-icon class="empty-icon">hotel</mat-icon>
        <h3>No Available Rooms</h3>
        <p>All rooms are currently full or undergoing maintenance.</p>
      </div>

      <div class="grid" *ngIf="!loading() && rooms().length > 0">
        <mat-card *ngFor="let r of rooms()" class="avail-card">
          <mat-card-header>
            <div mat-card-avatar class="slot-badge">
              {{ r.availableSlots }}
            </div>
            <mat-card-title>Room {{ r.roomNumber }}</mat-card-title>
            <mat-card-subtitle>Floor {{ r.floor }} &bull; {{ r.roomType }}</mat-card-subtitle>
          </mat-card-header>

          <mat-card-content>
            <div class="stat-highlight">
              <span class="slot-number">{{ r.availableSlots }}</span>
              <span class="slot-text">of {{ r.capacity }} beds vacant</span>
            </div>

            <div class="rent-row">
              <span class="rent-label">Monthly Rent:</span>
              <span class="rent-val">{{ r.monthlyRent | currencyInr }}</span>
            </div>
          </mat-card-content>

          <mat-card-actions align="end">
            <a mat-raised-button color="primary" [routerLink]="['/tenants/new']">
              <mat-icon>person_add</mat-icon> Allocate Bed
            </a>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .availability-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    .header {
      margin-bottom: 24px;
      .back-link { margin-bottom: 8px; }
      h1 { margin: 0; font-size: 26px; }
      .subtitle { margin: 4px 0 0; color: #666; }
    }
    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 20px;
    }
    .avail-card {
      border-radius: 12px;
      border-top: 4px solid #2e7d32;
    }
    .slot-badge {
      background: #e8f5e9;
      color: #2e7d32;
      font-weight: 700;
      font-size: 18px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 50%;
    }
    .stat-highlight {
      display: flex;
      align-items: baseline;
      gap: 8px;
      margin: 16px 0;
      .slot-number { font-size: 32px; font-weight: 700; color: #2e7d32; }
      .slot-text { color: #555; font-size: 14px; }
    }
    .rent-row {
      display: flex;
      justify-content: space-between;
      border-top: 1px solid #eee;
      padding-top: 8px;
      .rent-label { color: #666; }
      .rent-val { font-weight: bold; color: #1976d2; }
    }
    .empty-state {
      text-align: center;
      padding: 60px 20px;
      .empty-icon { font-size: 64px; height: 64px; width: 64px; color: #bbb; }
      h3 { margin-top: 16px; color: #555; }
    }
  `]
})
export class RoomAvailabilityComponent implements OnInit {
  private roomsService = inject(RoomsService);

  rooms = signal<RoomAvailability[]>([]);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    this.roomsService.getAvailableRooms().subscribe({
      next: (res) => {
        this.rooms.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}
