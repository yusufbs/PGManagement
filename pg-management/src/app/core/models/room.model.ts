export type RoomType = 'Single' | 'Double' | 'Triple' | 'Dormitory';
export type RoomStatus = 'Available' | 'Occupied' | 'Maintenance';

export interface Room {
  roomId: number;
  roomNumber: string;
  floor: number;
  roomType: RoomType | string;
  capacity: number;
  monthlyRent: number;
  amenities?: string;
  status: RoomStatus | string;
  currentOccupants?: number;
  isDeleted?: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface RoomAvailability {
  roomId: number;
  roomNumber: string;
  floor: number;
  roomType: string;
  capacity: number;
  currentOccupants: number;
  availableSlots: number;
  monthlyRent: number;
}

export interface CreateRoomRequest {
  roomNumber: string;
  floor: number;
  roomType: string;
  capacity: number;
  monthlyRent: number;
  amenities?: string;
}

export interface UpdateRoomRequest {
  roomNumber: string;
  floor: number;
  roomType: string;
  capacity: number;
  monthlyRent: number;
  amenities?: string;
}

export interface UpdateRoomStatusRequest {
  status: string;
}
