import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-role-manager',
  templateUrl: './role-manager.component.html',
  styleUrls: ['./role-manager.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule]
})
export class RoleManagerComponent {
  userRolesForm: FormGroup;
  roleUsersForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) {
    this.userRolesForm = this.fb.group({
      userId: ['', [Validators.required]]
    });

    this.roleUsersForm = this.fb.group({
      roleName: ['', [Validators.required]]
    });
  }

  viewUserRoles() {
    if (this.userRolesForm.valid) {
      const userId = this.userRolesForm.get('userId')?.value;
      this.router.navigate(['/admin/roles/user', userId]);
    }
  }

  viewRoleUsers() {
    if (this.roleUsersForm.valid) {
      const roleName = this.roleUsersForm.get('roleName')?.value;
      this.router.navigate(['/admin/roles/role', roleName]);
    }
  }
}