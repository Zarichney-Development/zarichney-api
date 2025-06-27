// src/app/cookbook/order/order.service.ts
import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { map, Observable, of, switchMap, tap, catchError, shareReplay } from 'rxjs';
import { Order, OrderStatus, PaymentStatus } from './order.model';
import * as OrderActions from './store/order.actions';
import { selectOrderById } from './store/order.selectors';
import { ApiService, PdfOptions } from '../../services/api.service';
import { PaymentService } from '../../services/payment.service';
import { HttpParams } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class OrderService {
    private readonly BASE_ENDPOINT = '/cookbook/order';
    // Add cache map to prevent duplicate API requests
    private orderCache = new Map<string, Observable<Order>>();

    constructor(
        private store: Store,
        private apiService: ApiService,
        private paymentService: PaymentService
    ) { }

    getOrder(orderId: string): Observable<Order> {
        // Check cache first to prevent duplicate requests
        if (this.orderCache.has(orderId)) {
            return this.orderCache.get(orderId)!;
        }

        // Dispatch the load action
        this.store.dispatch(OrderActions.loadOrder({ orderId }));

        // Get the order from store or API, and cache the result
        const order$ = this.store.select(selectOrderById(orderId)).pipe(
            switchMap(order => {
                if (!order) {
                    // If order is not in store, fetch it from API
                    return this.fetchOrder(orderId);
                }
                return of(order);
            }),
            // Share the same observable for multiple subscribers
            shareReplay(1),
            // Ensure errors propagate properly
            catchError(error => {
                console.error(`Error in getOrder for ${orderId}:`, error);
                // Remove failed request from cache
                this.orderCache.delete(orderId);
                // Re-throw the error
                throw error;
            })
        );

        // Store in cache
        this.orderCache.set(orderId, order$);

        return order$;
    }

    fetchOrder(orderId: string): Observable<Order> {
        // Add special debug for cookbook order fetch
        console.log(`🍳 Attempting to fetch cookbook order: ${orderId}`);

        return this.apiService.get<Order>(`${this.BASE_ENDPOINT}/${orderId}`, {
            // Force withCredentials to ensure cookies are sent
            withCredentials: true
        }).pipe(
            tap(order => {
                console.log(`✅ Successfully fetched order ${orderId}:`, order);
            }),
            catchError(error => {
                console.error(`❌ Failed to fetch order ${orderId}:`, error);

                // Let the error propagate - it will be caught by the interceptor if it's a 401
                throw error;
            })
        );
    }

    createOrder(orderData: Partial<Order>): Observable<Order> {
        return this.apiService.post<Order>(this.BASE_ENDPOINT, orderData);
    }

    updateOrder(orderId: string, orderData: Partial<Order>): Observable<Order> {
        return this.apiService.patch<Order>(`${this.BASE_ENDPOINT}/${orderId}`, orderData);
    }

    viewPdf(orderId: string): Observable<void> {
        return this.downloadOrderPdf(orderId).pipe(
            map(blob => {
                const url = window.URL.createObjectURL(blob);
                window.open(url, '_blank');
                window.URL.revokeObjectURL(url);
            })
        );
    }

    downloadPdf(orderId: string, options?: PdfOptions): Observable<void> {
        return this.downloadOrderPdf(orderId, options).pipe(
            map(blob => {
                const url = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.target = '_blank';
                link.download = `cookbook-${orderId}.pdf`;
                link.click();
                window.URL.revokeObjectURL(url);
            })
        );
    }

    downloadOrderPdf(orderId: string, options: PdfOptions = {}): Observable<Blob> {
        const params = new HttpParams()
            .set('rebuild', options.rebuild ?? false)
            .set('email', options.email ?? false);

        return this.apiService.getBlob(`${this.BASE_ENDPOINT}/${orderId}/pdf`, {
            params
        });
    }

    processOrder(orderId: string): Observable<string> {
        return this.apiService.post<string>(`${this.BASE_ENDPOINT}/${orderId}`, {});
    }

    // Method for initiating payment with success and cancel URLs
    initiatePayment(orderId: string): Observable<void> {
        // Construct the success and cancel URLs
        const baseUrl = window.location.origin;
        const successUrl = `${baseUrl}/cookbook/order/${orderId}?success=true`;
        const cancelUrl = `${baseUrl}/cookbook/order/${orderId}?cancelled=true`;

        return this.paymentService.createCheckoutSession(orderId, { successUrl, cancelUrl }).pipe(
            tap(response => {
                this.paymentService.redirectToCheckout(response.checkoutUrl);
            }),
            map(() => void 0),
            catchError(error => {
                console.error('Payment initiation failed:', error);
                throw error;
            })
        );
    }

    // Method to check payment status
    checkPaymentStatus(orderId: string): Observable<PaymentStatus> {
        return this.paymentService.getPaymentStatus(orderId).pipe(
            map(response => {
                switch (response.status) {
                    case 'succeeded':
                        return PaymentStatus.Completed;
                    case 'pending':
                        return PaymentStatus.Pending;
                    case 'failed':
                        return PaymentStatus.Failed;
                    default:
                        return PaymentStatus.None;
                }
            })
        );
    }

    getOrderStatusLabel(status: OrderStatus): string {
        const statuses: { [key in OrderStatus]: string } = {
            [OrderStatus.Submitted]: 'Submitted',
            [OrderStatus.InProgress]: 'In Progress',
            [OrderStatus.Completed]: 'Completed',
            [OrderStatus.Paid]: 'Paid',
            [OrderStatus.Failed]: 'Failed',
            [OrderStatus.AwaitingPayment]: 'Awaiting Payment',
        };
        return statuses[status] || 'Unknown';
    }

    getOrderStatusClass(status: OrderStatus): string {
        const classes = {
            [OrderStatus.Submitted]: 'submitted',
            [OrderStatus.InProgress]: 'in-progress',
            [OrderStatus.Completed]: 'completed',
            [OrderStatus.Paid]: 'paid',
            [OrderStatus.Failed]: 'failed',
            [OrderStatus.AwaitingPayment]: 'awaiting-payment'
        };
        return classes[status] || '';
    }
}