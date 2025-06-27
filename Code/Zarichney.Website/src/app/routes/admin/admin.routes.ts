import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ApiKeyListComponent } from './api-key-list/api-key-list.component';
import { CreateApiKeyComponent } from './create-api-key/create-api-key.component';
import { RoleManagerComponent } from './role-manager/role-manager.component';
import { UserRolesComponent } from './user-roles/user-roles.component';
import { RoleUsersComponent } from './role-users/role-users.component';

export const ADMIN_ROUTES: Routes = [
  { path: '', component: DashboardComponent, title: 'Admin Dashboard' },
  { path: 'api-keys', component: ApiKeyListComponent, title: 'Manage API Keys' },
  { path: 'api-keys/create', component: CreateApiKeyComponent, title: 'Create API Key' },
  { path: 'roles', component: RoleManagerComponent, title: 'Manage Roles' },
  { path: 'roles/user/:userId', component: UserRolesComponent, title: 'User Roles' },
  { path: 'roles/role/:roleName', component: RoleUsersComponent, title: 'Users in Role' },
];
