import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ADMIN_ROUTES } from './admin.routes';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

// Import all standalone components we need to configure in this module
import { DashboardComponent } from './dashboard/dashboard.component';
import { ApiKeyListComponent } from './api-key-list/api-key-list.component';
import { CreateApiKeyComponent } from './create-api-key/create-api-key.component';
import { RoleManagerComponent } from './role-manager/role-manager.component';
import { UserRolesComponent } from './user-roles/user-roles.component';
import { RoleUsersComponent } from './role-users/role-users.component';

@NgModule({
  imports: [
    CommonModule,
    HttpClientModule,
    ReactiveFormsModule,
    RouterModule.forChild(ADMIN_ROUTES)
  ]
})
export class AdminModule { }