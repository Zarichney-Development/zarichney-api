import { Component, OnInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { LoggingService } from '../../../services/log.service';
import { Actions, ofType } from '@ngrx/effects';
import { take, tap, delay } from 'rxjs/operators';
import * as AuthActions from '../store/auth.actions';

@Component({
  selector: 'app-email-confirmed',
  templateUrl: './email-confirmed.component.html',
  styleUrls: ['./email-confirmed.component.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class EmailConfirmedComponent implements OnInit {

  constructor(
    // private route: ActivatedRoute,
    private router: Router,
    private toastr: ToastrService,
    private log: LoggingService,
    private store: Store,
    private actions$: Actions,
    private ngZone: NgZone
  ) { }

  ngOnInit(): void {
    this.log.debug('EmailConfirmedComponent initialized.');
    this.log.debug('Dispatching checkAuth action');
    this.store.dispatch(AuthActions.checkAuth());

    // Success notification with explicit navigation if effect navigation fails
    this.actions$.pipe(
      ofType(AuthActions.checkAuthSuccess),
      take(1),
      tap(() => {
        this.toastr.success('Email confirmed successfully!', 'Success');
        this.log.debug('Email confirmation successful, dispatching UI flow complete');

        // Dispatch the UI flow complete action
        this.store.dispatch(AuthActions.uiFlowComplete({
          source: 'email-confirmation',
          returnUrl: null
        }));
      }),
      // Add a small delay and then force navigation as backup
      delay(1000),
      tap(() => {
        this.log.debug('Force navigating to home as backup');
        this.ngZone.run(() => {
          this.router.navigate(['/'], { replaceUrl: true });
        });
      })
    ).subscribe();

    // Set a fallback timeout in case nothing else works
    setTimeout(() => {
      this.log.debug('Fallback timeout triggered - forcing navigation to home');
      window.location.href = '/';
    }, 5000);
  }
}
