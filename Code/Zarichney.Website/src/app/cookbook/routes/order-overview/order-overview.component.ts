// src/app/components/order-overview/order-overview.component.ts
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { OrderService } from '../../order/order.service';
import { Order, OrderStatus, SynthesizedRecipe } from '../../order/order.model';
import { Observable, of, EMPTY } from 'rxjs';
import { RecipeCarouselComponent } from '../../components/recipe-carousel/recipe-carousel.component';
import { PaymentButtonComponent } from '../../components/payment-button/payment-button.component';
import { ClickOutsideDirective } from '../../../directives/click-outside.directive';
import { animate, style, transition, trigger } from '@angular/animations';
import { ToastrService, ToastrModule } from 'ngx-toastr';
import { AuthService } from '../../../services/auth.service';

@Component({
    selector: 'order-overview',
    standalone: true,
    imports: [
        CommonModule,
        RecipeCarouselComponent,
        PaymentButtonComponent,
        ClickOutsideDirective,
        ToastrModule
    ],
    providers: [
        OrderService,
    ],
    templateUrl: './order-overview.component.html',
    styleUrls: ['./order-overview.component.scss'],
    animations: [
        trigger('expandCollapse', [
            transition(':enter', [
                style({ height: 0, opacity: 0 }),
                animate('300ms ease-out',
                    style({ height: '*', opacity: 1 }))
            ]),
            transition(':leave', [
                style({ height: '*', opacity: 1 }),
                animate('300ms ease-out',
                    style({ height: 0, opacity: 0 }))
            ])
        ])
    ]
})
export class OrderOverviewComponent implements OnInit {
    orderId!: string;
    order$!: Observable<Order>;
    downloading = false;
    processing = false;
    menuOpen = false;
    isProgressExpanded = false;
    errorMessage: string | null = null;
    orderNotFound = false;
    loading = true; // Add loading state tracking

    private toastr = inject(ToastrService);

    constructor(
        private route: ActivatedRoute,
        private orderService: OrderService,
        private authService: AuthService, // Add authService 
    ) {
        // Immediately check auth status
        this.authService.checkAuthentication().subscribe({
            next: (response) => {
                console.log('⚡ Auth check on order page load:', response);
                if (!response.isAuthenticated) {
                    console.warn('🔒 User is not authenticated, prepare for redirection if API call fails');
                }
            },
            error: (err) => {
                console.error('🔒 Auth check failed:', err);
            }
        });
    }

    processOrder(): void {
        if (this.processing) return;
        this.processing = true;
        this.errorMessage = null;

        this.orderService.processOrder(this.orderId)
            .subscribe({
                next: () => {
                    this.processing = false;
                    this.toastr.success('Order processing started successfully');
                },
                error: (error) => {
                    console.error('Failed to process order:', error);
                    this.processing = false;
                    this.errorMessage = error;
                }
            });
    }

    // Method to handle payment initiation
    initiatePayment(): void {
        if (this.processing) return;
        this.processing = true;
        this.errorMessage = null;

        this.orderService.initiatePayment(this.orderId)
            .subscribe({
                next: () => {
                    this.processing = false;
                    this.toastr.success('Payment initiated successfully');
                },
                error: (error) => {
                    console.error('Failed to initiate payment:', error);
                    this.processing = false;
                    this.errorMessage = error?.message || 'Failed to initiate payment. Please try again.';
                }
            });
    }

    ngOnInit(): void {
        this.orderId = this.route.snapshot.paramMap.get('orderId') || '';
        const cancelled = this.route.snapshot.queryParamMap.get('cancelled');
        const success = this.route.snapshot.queryParamMap.get('success');

        console.log(`OrderOverviewComponent initialized with orderId: ${this.orderId}`);

        if (this.orderId) {
            try {
                // Remove the assignment to order$ to prevent duplicate requests
                // and only fetch the order once
                const orderRequest = this.orderService.getOrder(this.orderId);

                // Subscribe to fetch the order and handle errors
                const subscription = orderRequest.subscribe({
                    next: (order) => {
                        console.log('Order loaded successfully:', order);
                        this.loading = false;
                        // Now assign to order$ only when successful
                        this.order$ = of(order);
                    },
                    error: (error) => {
                        console.error('❌ Error loading order:', error);
                        this.loading = false;

                        // Extract the error message from the API response
                        let serverErrorMessage: string | undefined;
                        if (error?.error?.error?.message) {
                            serverErrorMessage = error.error.error.message;
                        }

                        // Handle 404 Not Found errors
                        if (error?.status === 404) {
                            this.orderNotFound = true;
                            this.errorMessage = serverErrorMessage || `Order not found: ${this.orderId}`;
                            this.toastr.error(this.errorMessage, 'Not Found');

                            // Create an empty observable for the template
                            this.order$ = EMPTY;
                        } else {
                            // Handle other errors
                            this.errorMessage = serverErrorMessage || error.message || 'Unknown error';
                            this.toastr.error(`Failed to load order: ${this.errorMessage}`, 'Error');

                            // Create an empty observable for the template
                            this.order$ = EMPTY;
                        }
                    }
                });

                // Clean up subscription when component is destroyed
                setTimeout(() => {
                    subscription.unsubscribe();
                }, 5000);
            } catch (e) {
                console.error('Exception in order component initialization:', e);
                this.errorMessage = 'An unexpected error occurred initializing the order view.';
            }

            // Handle query parameters
            if (cancelled === 'true') {
                this.errorMessage = 'Payment was cancelled. You can try again when you\'re ready.';
                // Optionally show a more user-friendly message with toastr
                setTimeout(() => {
                    this.toastr.warning('Payment was cancelled', 'Payment Status');
                }, 0);
            }

            // Handle the success query parameter
            if (success === 'true') {
                // Show a success message - no error message since it's successful
                setTimeout(() => {
                    this.toastr.success('Payment was successful! Your order is now marked as paid.', 'Payment Complete');
                }, 0);

                // Optionally, refresh the order to get the updated payment status
                setTimeout(() => {
                    this.order$ = this.orderService.getOrder(this.orderId);
                }, 1000);
            }
        } else {
            console.error('No orderId provided in the route');
            this.errorMessage = 'No order ID was provided. Please check the URL and try again.';
        }
    }

    toggleMenu(): void {
        this.menuOpen = !this.menuOpen;
    }

    closeMenu(): void {
        this.menuOpen = false;
    }

    toggleProgress(): void {
        this.isProgressExpanded = !this.isProgressExpanded;
    }

    onAccountDetails(): void {
        console.log('Account Details clicked');
        this.closeMenu();
    }

    getOrderStatus(status: OrderStatus): string {
        return this.orderService.getOrderStatusLabel(status);
    }

    getStatusClass(status: OrderStatus): string {
        return this.orderService.getOrderStatusClass(status);
    }

    getRecipeProgressPercentage(synthesizedRecipes: SynthesizedRecipe[]): number {
        if (!synthesizedRecipes?.length) return 0;
        return (synthesizedRecipes.length / 5) * 100;
    }

    isRecipeSynthesized(recipeName: string, synthesizedRecipes: SynthesizedRecipe[]): boolean {
        return synthesizedRecipes.some(recipe => recipe.title === recipeName);
    }

    trackByRecipeTitle(index: number, recipe: SynthesizedRecipe): string {
        return recipe.title;
    }

    async viewPdf(): Promise<void> {
        if (this.processing) return;
        this.closeMenu();
        this.processing = true;
        this.orderService.viewPdf(this.orderId)
            .subscribe({
                next: () => {
                    this.processing = false;
                },
                error: (error) => {
                    console.error('Failed to view PDF:', error);
                    this.processing = false;
                }
            });
    }

    async downloadPdf(rebuild: boolean = false): Promise<void> {
        if (this.processing) return;
        this.closeMenu();
        this.processing = true;
        this.orderService.downloadPdf(this.orderId, { rebuild })
            .subscribe({
                next: () => {
                    this.processing = false;
                },
                error: (error) => {
                    console.error('Failed to download PDF:', error);
                    this.processing = false;
                }
            });
    }
}