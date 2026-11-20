export interface Allocation {
  allocationId: number;
  roomId: number;
  tenantId: number;
  roomNumber?: string;
  tenantName?: string;
  checkInDate: string;
  checkOutDate?: string;
  securityDeposit: number;
  status: 'Active' | 'CheckedOut' | string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateAllocationRequest {
  roomId: number;
  tenantId: number;
  checkInDate: string;
  securityDeposit: number;
}

export interface CheckoutRequest {
  checkOutDate: string;
}
