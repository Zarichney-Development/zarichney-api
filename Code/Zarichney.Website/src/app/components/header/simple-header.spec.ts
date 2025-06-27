import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

// Mock LogoComponent for testing
@Component({
  selector: 'app-logo',
  template: '<div>Logo</div>',
  standalone: true
})
class MockLogoComponent {}

// Simple test component to verify setup
@Component({
  selector: 'app-test-header',
  template: `
    <div class="header-container">
      <div class="logo-container">
        <a routerLink="/">
          <app-logo></app-logo>
        </a>
      </div>
      
      <div class="auth-container">
        <ng-container *ngIf="isAuthenticated; else loginButton">
          <span class="welcome-message">Welcome, {{ userEmail }}</span>
          <button class="logout-button" (click)="logout()">Logout</button>
        </ng-container>
        
        <ng-template #loginButton>
          <a class="login-button" routerLink="/auth/login">Login</a>
        </ng-template>
      </div>
    </div>
  `,
  standalone: true,
  imports: [CommonModule, RouterTestingModule, MockLogoComponent]
})
class TestHeaderComponent {
  @Input() isAuthenticated = false;
  @Input() userEmail = '';
  
  logout() {
    // Test logout method
  }
}

describe('Header Component Setup Test', () => {
  let component: TestHeaderComponent;
  let fixture: ComponentFixture<TestHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TestHeaderComponent,
        RouterTestingModule.withRoutes([]),
        MockLogoComponent
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TestHeaderComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render without errors', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement).toBeTruthy();
  });

  it('should display login button when user is not authenticated', () => {
    component.isAuthenticated = false;
    fixture.detectChanges();
    
    const loginButton = fixture.nativeElement.querySelector('.login-button');
    const logoutButton = fixture.nativeElement.querySelector('.logout-button');
    
    expect(loginButton).toBeTruthy();
    expect(logoutButton).toBeFalsy();
  });

  it('should display welcome message when user is authenticated', () => {
    component.isAuthenticated = true;
    component.userEmail = 'test@example.com';
    fixture.detectChanges();
    
    const welcomeMessage = fixture.nativeElement.querySelector('.welcome-message');
    const logoutButton = fixture.nativeElement.querySelector('.logout-button');
    const loginButton = fixture.nativeElement.querySelector('.login-button');
    
    expect(welcomeMessage).toBeTruthy();
    expect(welcomeMessage.textContent).toContain('test@example.com');
    expect(logoutButton).toBeTruthy();
    expect(loginButton).toBeFalsy();
  });

  it('should have logo component', () => {
    fixture.detectChanges();
    
    const logoElement = fixture.nativeElement.querySelector('app-logo');
    expect(logoElement).toBeTruthy();
  });
});