import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { LoggingService } from '../../../services/log.service';
import { RoleCommandResult, RoleRequest } from '../../../models/auth.models';
import { Observable, catchError, map, of, switchMap, tap, finalize, BehaviorSubject } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-user-roles',
  templateUrl: './user-roles.component.html',
  styleUrls: ['./user-roles.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule]
})
export class UserRolesComponent implements OnInit {
  private rolesSubject = new BehaviorSubject<string[]>([]);
  roles$: Observable<string[]> = this.rolesSubject.asObservable();
  isLoading = true;
  error: string | null = null;
  userId: string | null = null;
  addRoleForm: FormGroup;

  constructor(
    private authService: AuthService,
    private log: LoggingService,
    private route: ActivatedRoute,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.addRoleForm = this.fb.group({
      roleName: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    // Set initial state
    this.isLoading = true;
    this.error = null;
    this.log.debug('Loading user roles... isLoading set to true');
    
    // Force change detection for the loading state
    this.cdr.detectChanges();
    
    // Get user ID from route parameters and load roles
    this.route.paramMap.pipe(
      map(params => {
        const userId = params.get('userId');
        this.log.debug('User ID from route params:', userId);
        return userId;
      }),
      tap(userId => {
        this.userId = userId;
        if (!userId) {
          this.log.error('User ID is required but not found in route params');
          throw new Error('User ID is required');
        }
      }),
      switchMap(userId => {
        this.log.debug('Fetching roles for user:', userId);
        return this.authService.getUserRoles(userId!).pipe(
          tap(result => {
            this.log.debug('Received user roles result:', result);
          }),
          map((result: RoleCommandResult) => {
            if (!result.success) {
              this.log.error('API returned error for user roles:', result.message);
              throw new Error(result.message || 'Failed to retrieve user roles');
            }
            return result.roles || [];
          }),
          catchError(error => {
            this.log.error('Error fetching user roles', error);
            this.error = error.message || 'An error occurred while fetching user roles';
            return of([]);
          })
        );
      }),
      // We'll handle loading state in the subscribe callbacks instead
      finalize(() => {
        this.log.debug('Observable chain finalized');
      })
    ).subscribe({
      next: (roles) => {
        this.log.debug('Successfully received roles data', { count: roles.length, roles });
        
        // Update the BehaviorSubject with new roles
        this.rolesSubject.next(roles);
        
        // Complete the loading state and force change detection
        this.log.debug('Setting isLoading to false in next handler');
        this.isLoading = false;
        
        // Force change detection to update the view
        this.cdr.detectChanges();
        this.log.debug('Change detection triggered in next handler');
        
        // Log the current component state for debugging
        setTimeout(() => {
          this.log.debug('Component state after timeout:', { 
            isLoading: this.isLoading, 
            hasError: !!this.error,
            userId: this.userId,
            roleCount: roles.length
          });
        }, 0);
      },
      error: (err) => {
        this.log.error('Error in loadRoles subscribe handler', err);
        
        // Update error state and clear roles
        this.error = err.message || 'An error occurred while loading user roles';
        this.rolesSubject.next([]);
        
        // Complete the loading state and force change detection
        this.log.debug('Setting isLoading to false in error handler');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  addRole(): void {
    if (this.addRoleForm.invalid || !this.userId) {
      return;
    }

    const roleName = this.addRoleForm.get('roleName')?.value;
    const roleRequest: RoleRequest = {
      userId: this.userId,
      roleName: roleName
    };

    this.authService.addUserToRole(roleRequest).subscribe({
      next: (result) => {
        if (result.success) {
          this.toastr.success('Role added successfully.');
          this.loadRoles();
          this.addRoleForm.reset();
        } else {
          this.toastr.error(result.message || 'Failed to add role.');
          this.log.error('Failed to add role', result);
        }
      },
      error: (error) => {
        this.toastr.error('Failed to add role.', 'Error');
        this.log.error('Error adding role', error);
      }
    });
  }

  removeRole(roleName: string): void {
    if (!this.userId) {
      return;
    }

    if (!confirm('Are you sure you want to remove this role from the user?')) {
      return;
    }

    const roleRequest: RoleRequest = {
      userId: this.userId,
      roleName: roleName
    };

    this.authService.removeUserFromRole(roleRequest).subscribe({
      next: (result) => {
        if (result.success) {
          this.toastr.success('Role removed successfully.');
          this.loadRoles();
        } else {
          this.toastr.error(result.message || 'Failed to remove role.');
          this.log.error('Failed to remove role', result);
        }
      },
      error: (error) => {
        this.toastr.error('Failed to remove role.', 'Error');
        this.log.error('Error removing role', error);
      }
    });
  }
}