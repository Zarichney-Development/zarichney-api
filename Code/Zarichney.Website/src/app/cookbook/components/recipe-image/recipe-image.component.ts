import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-recipe-image',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="recipe-image" [class.loading]="loading">
            <img
                [src]="imageUrl || fallbackUrl"
                (load)="onImageLoad()"
                (error)="onImageError()"
                [alt]="alt"
            />
            <div class="loading-overlay" *ngIf="loading">
                <span class="loader"></span>
            </div>
        </div>
    `,
    styles: [`
        .recipe-image {
            position: relative;
            width: 100%;
            height: 200px;
            overflow: hidden;
            border-radius: 8px 8px 0 0;
            background: var(--surface-bg, #f5f5f5);

            img {
                width: 100%;
                height: 100%;
                object-fit: cover;
                transition: opacity 0.3s ease;
            }

            &.loading img {
                opacity: 0;
            }
        }

        .loading-overlay {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            display: flex;
            align-items: center;
            justify-content: center;
            background: var(--card-bg, rgba(255,255,255,0.8));

            @media (prefers-color-scheme: dark) {
                background: rgba(45,45,45,0.8);
            }
        }

        .loader {
            width: 24px;
            height: 24px;
            border: 3px solid var(--surface-bg, #f3f3f3);
            border-top: 3px solid var(--text-secondary, #3498db);
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
    `]
})
export class RecipeImageComponent implements OnInit {
    @Input() imageUrl?: string;
    @Input() alt: string = 'Recipe image';

    loading = true;
    fallbackUrl = '/images/recipe-placeholder.jpg';

    ngOnInit() {
        this.loading = true;
    }

    onImageLoad() {
        this.loading = false;
    }

    onImageError() {
        this.loading = false;
        this.imageUrl = this.fallbackUrl;
    }
}
