export interface Tenant {
  tenantId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idProofType: string;
  idProofNumber: string;
  emergencyContact?: string;
  profilePhotoUrl?: string;
  isActive: boolean;
  isDeleted?: boolean;
  createdAt?: string;
  updatedAt?: string;
  currentRoomNumber?: string;
}

export interface TenantSummary {
  tenantId: number;
  fullName: string;
  email: string;
  phone: string;
  currentRoom?: string;
}

export interface CreateTenantRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idProofType: string;
  idProofNumber: string;
  emergencyContact?: string;
}

export interface UpdateTenantRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  idProofType: string;
  idProofNumber: string;
  emergencyContact?: string;
}
