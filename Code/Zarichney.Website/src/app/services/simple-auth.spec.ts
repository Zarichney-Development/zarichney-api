import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

// Simple test service to verify setup
class TestAuthService {
  private apiEndpoint = 'auth';

  constructor() {}

  login(email: string, password: string) {
    return of({ success: true, email });
  }

  logout() {
    return of({ success: true, message: 'Logged out successfully' });
  }

  checkAuthentication() {
    return of({ isAuthenticated: false, user: null });
  }
}

describe('Auth Service Setup Test', () => {
  let service: TestAuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TestAuthService]
    });
    service = TestBed.inject(TestAuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should login successfully', (done) => {
    const email = 'test@example.com';
    const password = 'password123';

    service.login(email, password).subscribe(response => {
      expect(response.success).toBe(true);
      expect(response.email).toBe(email);
      done();
    });
  });

  it('should logout successfully', (done) => {
    service.logout().subscribe(response => {
      expect(response.success).toBe(true);
      expect(response.message).toBe('Logged out successfully');
      done();
    });
  });

  it('should check authentication status', (done) => {
    service.checkAuthentication().subscribe(response => {
      expect(response.isAuthenticated).toBe(false);
      expect(response.user).toBe(null);
      done();
    });
  });
});