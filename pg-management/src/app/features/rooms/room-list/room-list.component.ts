import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RoomsService } from '../../../core/services/rooms.service';
import { Room } from '../../../core/models/room.model';
import { CurrencyInrPipe } from '../../../shared/pipes/currency-inr.pipe';
import { ConfirmationDialogComponent } from '../../../shared/components/confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-room-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatIconModule,
    MatChipsModule,
    MatProgressBarModule,
    MatDialogModule,
    CurrencyInrPipe
  ],
  template: `
    <div class="rooms-container">
      <div class="header-row">
        <div>
          <h1>Rooms Management</h1>
          <p class="subtitle">Monitor room occupancy, statuses, and allocate beds</p>
        </div>
        <div class="action-buttons">
          <a mat-stroked-button routerLink="/rooms/available" color="primary">
            <mat-icon>meeting_room</mat-icon> Show Empty Rooms
          </a>
          <a mat-raised-button routerLink="/rooms/new" color="primary">
            <mat-icon>add</mat-icon> Register Room
          </a>
        </div>
      </div>

      <!-- Status Filter Toggle -->
      <div class="filter-bar">
        <mat-button-toggle-group [value]="selectedStatus()" (change)="onStatusFilterChange($event.value)">
          <mat-button-toggle value="All">All Rooms</mat-button-toggle>
          <mat-button-toggle value="Available">Available</mat-button-toggle>
          <mat-button-toggle value="Occupied">Occupied</mat-button-toggle>
          <mat-button-toggle value="Maintenance">Maintenance</mat-button-toggle>
        </mat-button-toggle-group>
      </div>

      <div *ngIf="loading()" class="loading">
        <mat-progress-bar mode="indeterminate"></mat-progress-bar>
      </div>

      <!-- Room Cards Grid -->
      <div class="room-grid" *ngIf="!loading()">
        <mat-card
          *ngFor="let room of rooms()"
          class="room-card"
          [ngClass]="'status-border-' + room.status.toLowerCase()"
        >
          <mat-card-header>
            <div mat-card-avatar class="room-avatar" [ngClass]="'status-bg-' + room.status.toLowerCase()">
              {{ room.roomNumber }}
            </div>
            <mat-card-title>Room {{ room.roomNumber }}</mat-card-title>
            <mat-card-subtitle>Floor {{ room.floor }} &bull; {{ room.roomType }}</mat-card-subtitle>
          </mat-card-header>

          <mat-card-content>
            <div class="room-details">
              <div class="detail-row">
                <span class="label">Monthly Rent:</span>
                <span class="value rent">{{ room.monthlyRent | currencyInr }}</span>
              </div>
              <div class="detail-row">
                <span class="label">Capacity:</span>
                <span class="value">{{ room.capacity }} beds</span>
              </div>
              <div class="detail-row" *ngIf="room.amenities">
                <span class="label">Amenities:</span>
                <span class="value amenities">{{ room.amenities }}</span>
              </div>
            </div>

            <div class="status-chip-row">
              <span class="badge" [ngClass]="'badge-' + room.status.toLowerCase()">
                {{ room.status }}
              </span>
            </div>
          </mat-card-content>

          <mat-card-actions align="end">
            <button mat-icon-button [routerLink]="['/rooms', room.roomId, 'edit']" title="Edit Room">
              <mat-icon>edit</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="onDelete(room)" title="Delete Room">
              <mat-icon>delete</mat-icon>
            </button>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .rooms-container {
      padding: 24px;
      max-width: 1400px;
      margin: 0 auto;
    }
    .header-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 20px;
      flex-wrap: wrap;
      gap: 16px;
      h1 { margin: 0; font-size: 26px; }
      .subtitle { margin: 4px 0 0; color: #666; }
    }
    .action-buttons {
      display: flex;
      gap: 12px;
    }
    .filter-bar {
      margin-bottom: 24px;
    }
    .room-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 20px;
    }
    .room-card {
      border-radius: 12px;
      border-left: 6px solid #ccc;
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      &:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(0,0,0,0.1);
      }
    }
    .status-border-available { border-left-color: #2e7d32; }
    .status-border-occupied { border-left-color: #c62828; }
    .status-border-maintenance { border-left-color: #f57f17; }

    .room-avatar {
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      color: white;
      border-radius: 8px;
    }
    .status-bg-available { background-color: #2e7d32; }
    .status-bg-occupied { background-color: #c62828; }
    .status-bg-maintenance { background-color: #f57f17; }

    .room-details {
      margin: 12px 0;
      display: flex;
      flex-direction: column;
      gap: 6px;
    }
    .detail-row {
      display: flex;
      justify-content: space-between;
      font-size: 13px;
      .label { color: #666; }
      .value { font-weight: 500; }
      .rent { font-weight: 700; color: #1976d2; }
      .amenities { max-width: 180px; text-align: right; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    }
    .badge {
      display: inline-block;
      padding: 3px 10px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;
      text-transform: uppercase;
    }
    .badge-available { background: #e8f5e9; color: #2e7d32; }
    .badge-occupied { background: #ffebee; color: #c62828; }
    .badge-maintenance { background: #fffde7; color: #f57f17; }
  `]
})
export class RoomListComponent implements OnInit {
  private roomsService = inject(RoomsService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  rooms = signal<Room[]>([]);
  loading = signal<boolean>(true);
  selectedStatus = signal<string>('All');

  ngOnInit(): void {
    this.loadRooms();
  }

  loadRooms(): void {
    this.loading.set(true);
    const statusParam = this.selectedStatus() === 'All' ? undefined : this.selectedStatus();

    this.roomsService.getRooms(statusParam ? { status: statusParam } : undefined).subscribe({
      next: (res) => {
        this.rooms.set(res.data ?? []);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onStatusFilterChange(status: string): void {
    this.selectedStatus.set(status);
    this.loadRooms();
  }

  onDelete(room: Room): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      data: {
        title: 'Delete Room',
        message: `Are you sure you want to delete Room ${room.roomNumber}?`,
        confirmText: 'Delete',
        isDestructive: true
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.roomsService.deleteRoom(room.roomId).subscribe({
          next: () => {
            this.snackBar.open('Room deleted successfully.', 'Close', { duration: 3000 });
            this.loadRooms();
          }
        });
      }
    });
  }
}
