import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { Allocation, CheckoutRequest, CreateAllocationRequest } from '../models/allocation.model';

@Injectable({
  providedIn: 'root'
})
export class AllocationsService extends ApiService {
  getAllocations(): Observable<ApiResponse<Allocation[]>> {
    return this.get<ApiResponse<Allocation[]>>('allocations');
  }

  createAllocation(req: CreateAllocationRequest): Observable<ApiResponse<Allocation>> {
    return this.post<ApiResponse<Allocation>>('allocations', req);
  }

  checkoutTenant(allocationId: number, req: CheckoutRequest): Observable<ApiResponse<Allocation>> {
    return this.post<ApiResponse<Allocation>>(`allocations/${allocationId}/checkout`, req);
  }
}
