import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse } from '../models/api-response.model';
import { CreateTenantRequest, Tenant, TenantSummary, UpdateTenantRequest } from '../models/tenant.model';

@Injectable({
  providedIn: 'root'
})
export class TenantsService extends ApiService {
  getTenants(page: number = 1, pageSize: number = 10, search?: string): Observable<PagedResponse<Tenant>> {
    const params: any = { page, pageSize };
    if (search) {
      params.search = search;
    }
    return this.get<PagedResponse<Tenant>>('tenants', params);
  }

  getTenantSummaries(): Observable<ApiResponse<TenantSummary[]>> {
    return this.get<ApiResponse<TenantSummary[]>>('tenants/summary');
  }

  getTenantById(id: number): Observable<ApiResponse<Tenant>> {
    return this.get<ApiResponse<Tenant>>(`tenants/${id}`);
  }

  createTenant(req: CreateTenantRequest): Observable<ApiResponse<Tenant>> {
    return this.post<ApiResponse<Tenant>>('tenants', req);
  }

  updateTenant(id: number, req: UpdateTenantRequest): Observable<ApiResponse<Tenant>> {
    return this.put<ApiResponse<Tenant>>(`tenants/${id}`, req);
  }
}
