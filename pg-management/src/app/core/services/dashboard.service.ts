import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from './api.service';
import { DashboardStats } from '../models/dashboard.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService extends ApiService {
  getDashboard(): Observable<DashboardStats> {
    return this.get<ApiResponse<DashboardStats> | DashboardStats>('dashboard').pipe(
      map(res => (res && typeof res === 'object' && 'data' in res && res.data ? (res.data as DashboardStats) : (res as DashboardStats)))
    );
  }
}
