import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { MenuComponent } from '../../components/menu/menu.component';
import { LoggingService } from '../../services/log.service';
import { ApiService } from '../../services/api.service';
import { CommonModule, isPlatformBrowser } from '@angular/common';

@Component({
    selector: 'test-screen',
    standalone: true,
    imports: [MenuComponent, CommonModule],
    templateUrl: './test.component.html',
    styleUrls: ['./test.component.scss']
})
export class TestScreenComponent {
    healthTime: string | null = null;

    constructor(
        @Inject(PLATFORM_ID) private platformId: Object,
        private log: LoggingService,
        private apiService: ApiService
    ) {
    }

    ngOnInit() {
        setTimeout(() => {
            this.apiService.getHealth().subscribe({
                next: (response) => {
                    this.healthTime = response.time;
                },
                error: (err) => {
                    this.log.error('Health API Error:', err);
                },
                complete: () => {
                }
            });
        }, 0);

        if (isPlatformBrowser(this.platformId)) {
            console.log('TestComponent: Browser init');
        } else {
            console.log('TestComponent: Server init');
        }
    }

    ngAfterViewInit() {
        if (isPlatformBrowser(this.platformId)) {
            console.log('TestComponent: View initialized');
        }
    }
}
