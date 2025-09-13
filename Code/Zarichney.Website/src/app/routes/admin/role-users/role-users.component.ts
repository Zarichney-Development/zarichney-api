import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { UserRoleInfo } from '../../../models/auth.models';
import { Observable, BehaviorSubject, catchError, map, of, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-role-users',
  templateUrl: './role-users.component.html',
  styleUrls: ['./role-users.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class RoleUsersComponent implements OnInit {
  private usersSubject = new BehaviorSubject<UserRoleInfo[]>([]);
  users$: Observable<UserRoleInfo[]> = this.usersSubject.asObservable();
  isLoading = true;
  error: string | null = null;
  roleName: string | null = null;

  constructor(
    private authService: AuthService,
    private log: LoggingService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.isLoading = true;
    this.error = null;
    this.log.debug('RoleUsersComponent: Loading users started');
    
    this.route.paramMap.pipe(
      map(params => params.get('roleName')),
      tap(roleName => {
        this.roleName = roleName;
        this.log.debug(`RoleUsersComponent: Role name is ${roleName}`);
        if (!roleName) {
          throw new Error('Role name is required');
        }
      }),
      switchMap(roleName => this.authService.getUsersInRole(roleName!).pipe(
        tap(users => this.log.debug(`RoleUsersComponent: Loaded ${users.length} users for role ${roleName}`)),
        catchError(error => {
          this.log.error('RoleUsersComponent: Error fetching users in role', error);
          this.error = error.message || 'An error occurred while fetching users in role';
          return of([]);
        })
      )),
      catchError(error => {
        this.log.error('RoleUsersComponent: Error in users loading pipeline', error);
        this.error = error.message || 'An error occurred while loading users';
        this.usersSubject.next([]);
        return of([]);
      })
    ).subscribe({
      next: (users) => {
        this.log.debug(`RoleUsersComponent: Updating users subject with ${users.length} users`);
        this.usersSubject.next(users);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.log.error('RoleUsersComponent: Error in subscription handler', error);
        this.error = error.message || 'An unexpected error occurred';
        this.usersSubject.next([]);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      complete: () => {
        this.log.debug('RoleUsersComponent: Loading users completed');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }
}
