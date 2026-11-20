import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse, PagedResponse } from '../models/api-response.model';
import { Bill, BillFilter, GenerateBillRequest, PendingDue } from '../models/bill.model';

@Injectable({
  providedIn: 'root'
})
export class BillsService extends ApiService {
  getBills(filter?: BillFilter): Observable<PagedResponse<Bill>> {
    return this.get<PagedResponse<Bill>>('bills', filter);
  }

  getBillById(id: number): Observable<ApiResponse<Bill>> {
    return this.get<ApiResponse<Bill>>(`bills/${id}`);
  }

  generateBill(req: GenerateBillRequest): Observable<ApiResponse<Bill>> {
    return this.post<ApiResponse<Bill>>('bills/generate', req);
  }

  getPendingDues(): Observable<ApiResponse<PendingDue[]>> {
    return this.get<ApiResponse<PendingDue[]>>('bills/pending-dues');
  }

  printBill(id: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/bills/${id}/print`, { responseType: 'blob' });
  }
}
