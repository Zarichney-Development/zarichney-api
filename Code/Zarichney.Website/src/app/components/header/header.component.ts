import { Component } from '@angular/core';
import { LogoComponent } from './logo/logo.component';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { map } from 'rxjs/operators';

import { selectIsAuthenticated, selectUser } from '../../routes/auth/store/auth.selectors';
import * as AuthActions from '../../routes/auth/store/auth.actions';

@Component({
  selector: 'header',
  standalone: true,
  imports: [
    LogoComponent,
    RouterLink,
    CommonModule
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class AppHeaderComponent {
  isAuthenticated$ = this.store.select(selectIsAuthenticated);
  userEmail$ = this.store.select(selectUser).pipe(map(user => user?.email));

  constructor(private store: Store) { }

  logout(): void {
    this.store.dispatch(AuthActions.logout());
  }
}
