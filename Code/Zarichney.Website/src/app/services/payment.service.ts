import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../startup/environments';
import { LoggingService } from './log.service';

/**
 * Response from the checkout session endpoint
 */
interface CheckoutUrlResponse {
  checkoutUrl: string;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private apiUrl = environment.apiUrl;

  constructor(private log: LoggingService, private http: HttpClient) {
    this.log.verbose('PaymentService.constructor()', http);
  }

  /**
   * Creates a checkout session for the specified order and returns the checkout URL
   * @param orderId The ID of the order to create a checkout session for
   * @param options Additional options for the checkout session
   */
  createCheckoutSession(
    orderId: string,
    options: {
      successUrl?: string,
      cancelUrl?: string
    } = {}
  ): Observable<CheckoutUrlResponse> {
    return this.http.post<CheckoutUrlResponse>(
      `${this.apiUrl}/payment/create-checkout-session/${orderId}`,
      options
    );
  }

  /**
   * Navigates to the Stripe checkout page using the provided URL
   * @param checkoutUrl The URL to the Stripe checkout page
   */
  redirectToCheckout(checkoutUrl: string): void {
    window.location.href = checkoutUrl;
  }

  /**
   * Creates a checkout session for purchasing recipe credits
   * @param email The customer email
   * @param recipeCount The number of recipes to purchase
   */
  createCreditSession(
    email: string,
    recipeCount: number
  ): Observable<CheckoutUrlResponse> {
    return this.http.post<CheckoutUrlResponse>(
      `${this.apiUrl}/payment/create-credit-session`,
      { email, recipeCount }
    );
  }

  /**
   * Gets the status of a payment
   * @param orderId The ID of the order to get the payment status for
   */
  getPaymentStatus(orderId: string): Observable<{ status: string }> {
    return this.http.get<{ status: string }>(
      `${this.apiUrl}/payment/status/${orderId}`
    );
  }
}