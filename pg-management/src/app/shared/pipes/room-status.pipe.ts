import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'roomStatus',
  standalone: true
})
export class RoomStatusPipe implements PipeTransform {
  transform(status: string | null | undefined): { label: string; cssClass: string } {
    switch (status?.toLowerCase()) {
      case 'available':
        return { label: 'Available', cssClass: 'status-available' };
      case 'occupied':
        return { label: 'Occupied', cssClass: 'status-occupied' };
      case 'maintenance':
        return { label: 'Maintenance', cssClass: 'status-maintenance' };
      default:
        return { label: status ?? 'Unknown', cssClass: 'status-unknown' };
    }
  }
}
