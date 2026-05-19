export interface Payment {
  id: string;
  amount: number;
  currency: string;
  method: string;
  status: string;
  reference?: string;
  description?: string;
  createdAt: string;
  processedAt?: string;
  failureReason?: string;
}

export interface PaymentRequest {
  amount: number;
  currency: string;
  method: string;
  description?: string;
}

export interface RefundRequest {
  reason: string;
}
