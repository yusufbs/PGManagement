import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse } from '../models/api-response.model';
import { CreateRoomRequest, Room, RoomAvailability, UpdateRoomRequest, UpdateRoomStatusRequest } from '../models/room.model';

@Injectable({
  providedIn: 'root'
})
export class RoomsService extends ApiService {
  getRooms(filter?: { status?: string; page?: number; pageSize?: number }): Observable<PagedResponse<Room>> {
    return this.get<PagedResponse<Room>>('rooms', filter);
  }

  getAvailableRooms(): Observable<ApiResponse<RoomAvailability[]>> {
    return this.get<ApiResponse<RoomAvailability[]>>('rooms/available');
  }

  getRoomById(id: number): Observable<ApiResponse<Room>> {
    return this.get<ApiResponse<Room>>(`rooms/${id}`);
  }

  createRoom(req: CreateRoomRequest): Observable<ApiResponse<Room>> {
    return this.post<ApiResponse<Room>>('rooms', req);
  }

  updateRoom(id: number, req: UpdateRoomRequest): Observable<ApiResponse<Room>> {
    return this.put<ApiResponse<Room>>(`rooms/${id}`, req);
  }

  updateRoomStatus(id: number, status: string | UpdateRoomStatusRequest): Observable<ApiResponse<Room>> {
    const payload = typeof status === 'string' ? { status } : status;
    return this.patch<ApiResponse<Room>>(`rooms/${id}/status`, payload);
  }

  deleteRoom(id: number): Observable<ApiResponse<boolean>> {
    return this.delete<ApiResponse<boolean>>(`rooms/${id}`);
  }
}
