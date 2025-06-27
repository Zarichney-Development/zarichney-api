import { Component, Input, ViewChild, ElementRef, NgZone, OnDestroy, HostListener, PLATFORM_ID, Inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RecipeImageComponent } from '../recipe-image/recipe-image.component';
import { SynthesizedRecipe } from '../../order/order.model';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
    selector: 'app-recipe-carousel',
    standalone: true,
    imports: [CommonModule, RecipeImageComponent],
    templateUrl: './recipe-carousel.component.html',
    styleUrls: ['./recipe-carousel.component.scss'],
    animations: [
        trigger('fadeOut', [
            transition(':leave', [
                animate('300ms ease-out', style({ opacity: 0 }))
            ])
        ])
    ]
})
export class RecipeCarouselComponent implements OnDestroy {
    @Input() recipes: SynthesizedRecipe[] = [];
    @ViewChild('track') track!: ElementRef;
    currentIndex = 0;
    showSwipeHint = true;
    private touchStartX = 0;
    private touchMoveX = 0;
    private readonly swipeThreshold = 50;
    private viewportWidth = 0;
    private hintTimeout: any;
    private isBrowser: boolean;

    constructor(
        private ngZone: NgZone,
        @Inject(PLATFORM_ID) platformId: Object
    ) {
        this.isBrowser = isPlatformBrowser(platformId);
        if (this.isBrowser) {
            this.updateViewportWidth();
        }
    }

    ngOnInit() {
        // Show swipe hint for 3 seconds on mobile devices
        if (this.isBrowser && 'ontouchstart' in window) {
            this.hintTimeout = setTimeout(() => {
                this.showSwipeHint = false;
            }, 3000);
        } else {
            this.showSwipeHint = false;
        }
    }

    ngOnDestroy() {
        if (this.hintTimeout) {
            clearTimeout(this.hintTimeout);
        }
    }

    private updateViewportWidth() {
        if (this.isBrowser) {
            this.viewportWidth = window.innerWidth;
            this.adjustCarouselSize();
        }
    }

    @HostListener('window:resize')
    onResize() {
        if (this.isBrowser) {
            this.updateViewportWidth();
        }
    }

    private adjustCarouselSize() {
        if (this.isBrowser && this.track?.nativeElement) {
            const width = this.viewportWidth > 1200 ? 1000 :
                this.viewportWidth > 768 ? Math.min(this.viewportWidth * 0.9, 800) :
                    this.viewportWidth;
            this.track.nativeElement.style.width = `${width}px`;
        }
    }

    onTouchStart(event: TouchEvent) {
        this.touchStartX = event.touches[0].clientX;
        this.track.nativeElement.style.transition = 'none';
    }

    onTouchMove(event: TouchEvent) {
        if (!this.touchStartX) return;

        this.touchMoveX = event.touches[0].clientX;
        const deltaX = this.touchMoveX - this.touchStartX;
        const currentTransform = -(this.currentIndex * 100);

        // Calculate boundaries
        const maxTransform = 0;
        const minTransform = -((this.recipes.length - 1) * 100);

        // Apply resistance at edges
        let transform = currentTransform + (deltaX / this.track.nativeElement.offsetWidth * 100);
        if (transform > maxTransform) {
            transform = maxTransform + (transform - maxTransform) * 0.2;
        } else if (transform < minTransform) {
            transform = minTransform + (transform - minTransform) * 0.2;
        }

        this.track.nativeElement.style.transform = `translateX(${transform}%)`;
    }

    onTouchEnd() {
        if (!this.touchStartX || !this.touchMoveX) return;

        const deltaX = this.touchMoveX - this.touchStartX;
        const movePercentage = (deltaX / this.track.nativeElement.offsetWidth) * 100;

        this.track.nativeElement.style.transition = 'transform 0.3s ease-out';

        if (Math.abs(deltaX) > this.swipeThreshold) {
            if (deltaX > 0 && this.currentIndex > 0) {
                this.previous();
            } else if (deltaX < 0 && this.currentIndex < this.recipes.length - 1) {
                this.next();
            } else {
                this.goToSlide(this.currentIndex);
            }
        } else {
            this.goToSlide(this.currentIndex);
        }

        this.touchStartX = 0;
        this.touchMoveX = 0;
    }

    goToSlide(index: number) {
        this.currentIndex = index;
        this.track.nativeElement.style.transition = 'transform 0.3s ease-out';
        requestAnimationFrame(() => {
            this.track.nativeElement.style.transform = `translateX(-${index * 100}%)`;
        });
    }

    next() {
        if (this.currentIndex < this.recipes.length - 1) {
            this.goToSlide(this.currentIndex + 1);
        }
    }

    previous() {
        if (this.currentIndex > 0) {
            this.goToSlide(this.currentIndex - 1);
        }
    }

    onKeyDown(event: KeyboardEvent) {
        switch (event.key) {
            case 'ArrowLeft':
                this.previous();
                break;
            case 'ArrowRight':
                this.next();
                break;
        }
    }
}
