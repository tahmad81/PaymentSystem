import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { Payment, PaymentRequest, RefundRequest } from './models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly apiUrl = `${environment.apiUrl}/payments`;

  constructor(private readonly http: HttpClient) {}

  list(): Observable<Payment[]> {
    return this.http.get<Payment[]>(this.apiUrl);
  }

  get(id: string): Observable<Payment> {
    return this.http.get<Payment>(`${this.apiUrl}/${id}`);
  }

  create(request: PaymentRequest): Observable<Payment> {
    return this.http.post<Payment>(this.apiUrl, request);
  }

  update(id: string, request: PaymentRequest): Observable<Payment> {
    return this.http.put<Payment>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  refund(id: string, reason: string): Observable<Payment> {
    return this.http.post<Payment>(`${this.apiUrl}/${id}/refund`, { reason } as RefundRequest);
  }
}
