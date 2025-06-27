import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderStatus, PaymentStatus } from '../../order/order.model';

@Component({
  selector: 'app-payment-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-button.component.html',
  styleUrls: ['./payment-button.component.scss']
})
export class PaymentButtonComponent {
  @Input() requiresPayment: boolean = false;
  @Input() orderStatus: OrderStatus | undefined;
  @Input() paymentStatus: PaymentStatus = PaymentStatus.None;
  @Input() processing: boolean = false;
  @Output() paymentInitiated = new EventEmitter<void>();
  @Output() paymentRetry = new EventEmitter<void>();

  /**
   * Checks if the payment button should be displayed
   */
  get shouldShowPaymentButton(): boolean {
    return this.requiresPayment && 
           (this.orderStatus === OrderStatus.AwaitingPayment || 
            this.paymentStatus === PaymentStatus.Failed);
  }

  /**
   * Gets the payment button state class
   */
  get paymentStateClass(): string {
    if (this.processing) return 'processing';
    if (this.paymentStatus === PaymentStatus.Pending) return 'pending';
    if (this.paymentStatus === PaymentStatus.Failed) return 'failed';
    if (this.paymentStatus === PaymentStatus.Completed) return 'completed';
    return 'default';
  }

  /**
   * Gets the appropriate button text based on payment status
   */
  get buttonText(): string {
    if (this.processing) return 'Processing...';
    if (this.paymentStatus === PaymentStatus.Pending) return 'Payment Pending';
    if (this.paymentStatus === PaymentStatus.Failed) return 'Retry Payment';
    if (this.paymentStatus === PaymentStatus.Completed) return 'Payment Complete';
    return 'Pay Now';
  }

  /**
   * Gets the appropriate button icon based on payment status
   */
  get buttonIcon(): string {
    if (this.processing) return '⏳';
    if (this.paymentStatus === PaymentStatus.Pending) return '⌛';
    if (this.paymentStatus === PaymentStatus.Failed) return '❌';
    if (this.paymentStatus === PaymentStatus.Completed) return '✅';
    return '💳';
  }

  /**
   * Determines if the button should be disabled
   */
  get isButtonDisabled(): boolean {
    return this.processing || 
           this.paymentStatus === PaymentStatus.Pending || 
           this.paymentStatus === PaymentStatus.Completed;
  }

  /**
   * Handles the click event of the payment button
   */
  onPaymentButtonClick(): void {
    if (this.isButtonDisabled) return;
    
    if (this.paymentStatus === PaymentStatus.Failed) {
      this.paymentRetry.emit();
    } else {
      this.paymentInitiated.emit();
    }
  }
}