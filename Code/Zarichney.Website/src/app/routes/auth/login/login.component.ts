import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';

import * as AuthActions from '../store/auth.actions';
import { selectAuthLoading, selectAuthError } from '../../../routes/auth/store/auth.selectors';
import { LoggingService } from '../../../services/log.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule
  ]
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading$: Observable<boolean>;
  error$: Observable<string | null>;
  returnUrl: string | null = null;

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private route: ActivatedRoute,
    private logger: LoggingService
  ) {
    this.loading$ = this.store.select(selectAuthLoading);
    this.error$ = this.store.select(selectAuthError);
  }

  ngOnInit(): void {
    this.initForm();
    this.route.queryParamMap.pipe(
      take(1)
    ).subscribe(params => {
      const mode = params.get('mode');
      if (mode === 'email-confirmed') {
        this.returnUrl = '/';
      } else {
        this.returnUrl = params.get('returnUrl');
      }
      this.logger.debug('Login component final returnUrl:', this.returnUrl);
    });
  }

  private initForm(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const email = this.loginForm.get('email')?.value;
      const password = this.loginForm.get('password')?.value;

      this.logger.debug('Login form submitted with returnUrl:', this.returnUrl);

      this.store.dispatch(AuthActions.login({
        credentials: { email, password },
        returnUrl: this.returnUrl
      }));
    }
  }
}
