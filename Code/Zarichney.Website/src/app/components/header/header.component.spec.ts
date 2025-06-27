import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { provideMockStore, MockStore } from '@ngrx/store/testing';
import { By } from '@angular/platform-browser';
import { Component } from '@angular/core';

import { AppHeaderComponent } from './header.component';
import { selectIsAuthenticated, selectUser } from '../../routes/auth/store/auth.selectors';
import * as AuthActions from '../../routes/auth/store/auth.actions';

// Mock LogoComponent for testing
@Component({
  selector: 'app-logo',
  template: '<div>Logo</div>'
})
class MockLogoComponent {}

describe('AppHeaderComponent', () => {
  let component: AppHeaderComponent;
  let fixture: ComponentFixture<AppHeaderComponent>;
  let store: MockStore;

  const initialState = {
    auth: {
      isAuthenticated: false,
      user: null
    }
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AppHeaderComponent,
        RouterTestingModule.withRoutes([])
      ],
      declarations: [MockLogoComponent],
      providers: [
        provideMockStore({ initialState })
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppHeaderComponent);
    component = fixture.componentInstance;
    store = TestBed.inject(MockStore);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render without errors', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement).toBeTruthy();
  });

  it('should display login button when user is not authenticated', () => {
    store.overrideSelector(selectIsAuthenticated, false);
    store.overrideSelector(selectUser, null);
    
    fixture.detectChanges();
    
    const loginButton = fixture.debugElement.query(By.css('.login-button'));
    const logoutButton = fixture.debugElement.query(By.css('.logout-button'));
    
    expect(loginButton).toBeTruthy();
    expect(logoutButton).toBeFalsy();
  });

  it('should display welcome message and logout button when user is authenticated', () => {
    const mockUser = { email: 'test@example.com', id: '123' };
    
    store.overrideSelector(selectIsAuthenticated, true);
    store.overrideSelector(selectUser, mockUser);
    
    fixture.detectChanges();
    
    const welcomeMessage = fixture.debugElement.query(By.css('.welcome-message'));
    const logoutButton = fixture.debugElement.query(By.css('.logout-button'));
    const loginButton = fixture.debugElement.query(By.css('.login-button'));
    
    expect(welcomeMessage).toBeTruthy();
    expect(welcomeMessage.nativeElement.textContent).toContain('test@example.com');
    expect(logoutButton).toBeTruthy();
    expect(loginButton).toBeFalsy();
  });

  it('should dispatch logout action when logout button is clicked', () => {
    const mockUser = { email: 'test@example.com', id: '123' };
    
    store.overrideSelector(selectIsAuthenticated, true);
    store.overrideSelector(selectUser, mockUser);
    
    const dispatchSpy = jest.spyOn(store, 'dispatch');
    
    fixture.detectChanges();
    
    const logoutButton = fixture.debugElement.query(By.css('.logout-button'));
    logoutButton.nativeElement.click();
    
    expect(dispatchSpy).toHaveBeenCalledWith(AuthActions.logout());
  });

  it('should have logo component', () => {
    fixture.detectChanges();
    
    const logoElement = fixture.debugElement.query(By.css('app-logo'));
    expect(logoElement).toBeTruthy();
  });
});