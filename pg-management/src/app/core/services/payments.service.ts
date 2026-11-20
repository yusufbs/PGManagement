import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';
import { CreatePaymentRequest, Payment } from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentsService extends ApiService {
  createPayment(req: CreatePaymentRequest): Observable<ApiResponse<Payment>> {
    return this.post<ApiResponse<Payment>>('payments', req);
  }

  getPaymentsByTenant(tenantId: number): Observable<ApiResponse<Payment[]>> {
    return this.get<ApiResponse<Payment[]>>(`payments/tenant/${tenantId}`);
  }

  getPaymentsByBill(billId: number): Observable<ApiResponse<Payment[]>> {
    return this.get<ApiResponse<Payment[]>>(`payments/bill/${billId}`);
  }
}
