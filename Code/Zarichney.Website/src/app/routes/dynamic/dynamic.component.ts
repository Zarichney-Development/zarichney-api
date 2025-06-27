import { Component, OnInit } from '@angular/core';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { LoggingService } from '../../services/log.service';
import { selectAuthStatus, selectUserRoles, selectIsAuthenticated } from '../../routes/auth/store/auth.selectors';
import { take } from 'rxjs/operators';

@Component({
  selector: 'dynamic-screen',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './dynamic.component.html',
  styleUrls: ['./dynamic.component.scss']
})
export class DynamicScreenComponent implements OnInit {
  isDebugRoute = false;

  authDebugInfo: any = {
    status: 'Loading...',
    isAuthenticated: false,
    roles: []
  };

  routeDebugInfo: any = {
    url: '',
    path: '',
    params: {},
    queryParams: {},
    data: {}
  };

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private store: Store,
    private log: LoggingService
  ) { }

  ngOnInit(): void {
    // Check if this is a debug route
    this.route.data.subscribe(data => {
      this.isDebugRoute = !!data['debugRoute'];

      if (this.isDebugRoute) {
        this.collectDebugInfo();
      }
    });

    // Populate route debug info
    this.routeDebugInfo = {
      url: this.router.url,
      path: this.route.snapshot.routeConfig?.path,
      params: this.route.snapshot.params,
      queryParams: this.route.snapshot.queryParams,
      data: this.route.snapshot.data
    };

    this.log.info('DynamicScreenComponent initialized', {
      url: this.router.url,
      isDebugRoute: this.isDebugRoute
    });
  }

  private collectDebugInfo(): void {
    // Collect auth state info
    this.store.select(selectAuthStatus).pipe(take(1))
      .subscribe(status => {
        this.authDebugInfo.status = status;
        this.log.debug('Auth status in debug component:', status);
      });

    this.store.select(selectIsAuthenticated).pipe(take(1))
      .subscribe(isAuthenticated => {
        this.authDebugInfo.isAuthenticated = isAuthenticated;
        this.log.debug('Is authenticated in debug component:', isAuthenticated);
      });

    this.store.select(selectUserRoles).pipe(take(1))
      .subscribe(roles => {
        this.authDebugInfo.roles = roles;
        this.log.debug('User roles in debug component:', roles);
      });
  }

  navigateHome(event?: Event): void {
    if (event) {
      event.preventDefault();
    }

    // Try to navigate using the Angular router first
    this.router.navigate(['/'], { skipLocationChange: false })
      .then(success => {
        this.log.info('Router navigation result:', success);

        // If router navigation fails or returns false, use direct browser navigation
        if (!success) {
          this.log.info('Router navigation failed, using window.location instead');
          window.location.href = '/';
        }
      })
      .catch(error => {
        this.log.error('Router navigation error:', error);
        // Fallback to direct browser navigation
        window.location.href = '/';
      });

    this.log.info('Navigating to home from 404 page');
  }
}
