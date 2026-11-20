export type PaymentMode = 'Cash' | 'UPI' | 'BankTransfer' | 'Card';

export interface Payment {
  paymentId: number;
  billId: number;
  amountPaid: number;
  paymentDate: string;
  paymentMode: PaymentMode | string;
  transactionRef?: string;
  receivedBy?: string;
  remarks?: string;
}

export interface CreatePaymentRequest {
  billId: number;
  amountPaid: number;
  paymentMode: string;
  transactionRef?: string;
  receivedBy?: string;
  remarks?: string;
}
