import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Payment, PaymentRequest } from './models';
import { PaymentService } from './payment.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  payments: Payment[] = [];
  errorMessage = '';
  loading = false;
  createModel: PaymentRequest = { amount: 0, currency: 'USD', method: 'Card', description: '' };
  editModel: PaymentRequest = { amount: 0, currency: 'USD', method: 'Card', description: '' };
  selectedPaymentId: string | null = null;
  refundReason = '';

  constructor(private readonly paymentService: PaymentService) {}

  ngOnInit(): void {
    this.loadPayments();
  }

  loadPayments(): void {
    this.loading = true;
    this.paymentService.list().subscribe({
      next: payments => {
        this.payments = payments;
        this.loading = false;
      },
      error: err => {
        this.errorMessage = 'Unable to load payments.';
        console.error(err);
        this.loading = false;
      }
    });
  }

  createPayment(): void {
    this.errorMessage = '';
    this.paymentService.create(this.createModel).subscribe({
      next: () => {
        this.createModel = { amount: 0, currency: 'USD', method: 'Card', description: '' };
        this.loadPayments();
      },
      error: err => {
        this.errorMessage = 'Unable to create payment.';
        console.error(err);
      }
    });
  }

  editPayment(payment: Payment): void {
    this.selectedPaymentId = payment.id;
    this.editModel = {
      amount: payment.amount,
      currency: payment.currency,
      method: payment.method,
      description: payment.description ?? ''
    };
    this.errorMessage = '';
  }

  savePayment(): void {
    if (!this.selectedPaymentId) {
      return;
    }

    this.paymentService.update(this.selectedPaymentId, this.editModel).subscribe({
      next: () => {
        this.selectedPaymentId = null;
        this.editModel = { amount: 0, currency: 'USD', method: 'Card', description: '' };
        this.loadPayments();
      },
      error: err => {
        this.errorMessage = 'Unable to update payment. Only pending payments can be updated.';
        console.error(err);
      }
    });
  }

  cancelEdit(): void {
    this.selectedPaymentId = null;
    this.editModel = { amount: 0, currency: 'USD', method: 'Card', description: '' };
    this.errorMessage = '';
  }

  deletePayment(id: string): void {
    this.paymentService.delete(id).subscribe({
      next: () => this.loadPayments(),
      error: err => {
        this.errorMessage = 'Unable to delete payment. Only pending payments can be deleted.';
        console.error(err);
      }
    });
  }

  refundPayment(id: string): void {
    const reason = this.refundReason || 'Refund requested from client app.';
    this.paymentService.refund(id, reason).subscribe({
      next: () => {
        this.refundReason = '';
        this.loadPayments();
      },
      error: err => {
        this.errorMessage = 'Unable to refund payment. Only completed payments can be refunded.';
        console.error(err);
      }
    });
  }
}
