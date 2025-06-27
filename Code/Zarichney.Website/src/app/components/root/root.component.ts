import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { LoggingService } from '../../services/log.service';
import { SeoService } from '../../services/seo.service';
import { catchError, map, of, tap } from 'rxjs';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { HelperComponent } from '../../components/helper/helper.component';
import { RouterModule } from '@angular/router';
import { AuthStoreModule, provideAuthFeature } from '../../routes/auth/store/auth-store.module';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    HelperComponent,
    RouterModule
  ],
  templateUrl: './root.component.html',
  styleUrls: ['./root.component.scss']
})
export class RootComponent implements OnInit {
  title = 'zarichney-website';
  message!: string;
  error!: string;
  apiUrl!: string;

  constructor(
    private log: LoggingService,
    private apiService: ApiService,
    private seoService: SeoService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
  }

  $message = this.apiService.$message().pipe(
    tap(response => this.log.verbose('RootComponent.getMessage()', response)),
    map(response => response.message),
    catchError(error => {
      this.log.error('RootComponent.getMessage()', error);
      this.error = `Error ${error}`;
      return of();
    })
  );

  ngOnInit() {
    try {
      this.log.verbose('RootComponent.ngOnInit()', this);

      this.apiUrl = this.apiService.getApiUrl();

      // Apply default SEO settings for the entire app
      this.seoService.setDefaultTags();

      // Only execute browser-specific code when running in the browser
      if (isPlatformBrowser(this.platformId)) {
        // Initialize browser-only features here
      }

      this.log.verbose('RootComponent init complete', this);
    } catch (error) {
      this.log.error('RootComponent.ngOnInit() - catch', error);
      this.error = `Error ${error}`;
    }
  }

}