import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { AuthService } from '../../../Zarichney.Website/src/app/services/auth.service';
import { ApiService } from '../../../Zarichney.Website/src/app/services/api.service';
import { LoggingService } from '../../../Zarichney.Website/src/app/services/log.service';
import { LoginRequest, AuthResponse } from '../../../Zarichney.Website/src/app/models/auth.models';

describe('AuthService', () => {
  let service: AuthService;
  let mockApiService: jest.Mocked<ApiService>;
  let mockLogService: jest.Mocked<LoggingService>;

  beforeEach(() => {
    // Create mock services
    mockApiService = {
      post: jest.fn(),
      get: jest.fn(),
      delete: jest.fn(),
      getApiUrl: jest.fn().mockReturnValue('http://localhost:5000/api')
    } as unknown as jest.Mocked<ApiService>;

    mockLogService = {
      verbose: jest.fn(),
      info: jest.fn(),
      warn: jest.fn(),
      error: jest.fn()
    } as unknown as jest.Mocked<LoggingService>;

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: ApiService, useValue: mockApiService },
        { provide: LoggingService, useValue: mockLogService }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call verbose log on initialization', () => {
    expect(mockLogService.verbose).toHaveBeenCalledWith(
      'AuthService initialized', 
      'http://localhost:5000/api/auth'
    );
  });

  describe('login', () => {
    it('should call API service with correct parameters for login', () => {
      const mockCredentials: LoginRequest = {
        email: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: AuthResponse = {
        success: true,
        message: 'Login successful',
        email: 'test@example.com'
      };

      mockApiService.post.mockReturnValue(of(mockResponse));

      service.login(mockCredentials).subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      expect(mockApiService.post).toHaveBeenCalledWith(
        '/auth/login',
        mockCredentials,
        { withCredentials: true }
      );

      expect(mockLogService.verbose).toHaveBeenCalledWith(
        'AuthService.login',
        { email: 'test@example.com' }
      );
    });

    it('should log info on successful login response', () => {
      const mockCredentials: LoginRequest = {
        email: 'test@example.com',
        password: 'password123'
      };

      const mockResponse: AuthResponse = {
        success: true,
        message: 'Login successful',
        email: 'test@example.com'
      };

      mockApiService.post.mockReturnValue(of(mockResponse));

      service.login(mockCredentials).subscribe();

      expect(mockLogService.info).toHaveBeenCalledWith('Login response', mockResponse);
    });
  });

  describe('logout', () => {
    it('should call API service with correct parameters for logout', () => {
      const mockResponse: AuthResponse = {
        success: true,
        message: 'Logout successful'
      };

      mockApiService.post.mockReturnValue(of(mockResponse));

      service.logout().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      expect(mockApiService.post).toHaveBeenCalledWith(
        '/auth/logout',
        {},
        { withCredentials: true }
      );

      expect(mockLogService.verbose).toHaveBeenCalledWith('AuthService.logout');
    });
  });

  describe('refreshToken', () => {
    it('should call API service with correct parameters for token refresh', () => {
      const mockResponse: AuthResponse = {
        success: true,
        message: 'Token refreshed'
      };

      mockApiService.post.mockReturnValue(of(mockResponse));

      service.refreshToken().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      expect(mockApiService.post).toHaveBeenCalledWith(
        '/auth/refresh',
        {},
        { withCredentials: true }
      );

      expect(mockLogService.verbose).toHaveBeenCalledWith('AuthService.refreshToken');
    });
  });

  describe('checkAuthentication', () => {
    it('should call API service with correct parameters for auth check', () => {
      const mockResponse = {
        isAuthenticated: true,
        user: { email: 'test@example.com', id: '123' }
      };

      mockApiService.get.mockReturnValue(of(mockResponse));

      service.checkAuthentication().subscribe(response => {
        expect(response).toEqual(mockResponse);
      });

      expect(mockApiService.get).toHaveBeenCalledWith(
        '/auth/check-authentication',
        { withCredentials: true }
      );

      expect(mockLogService.verbose).toHaveBeenCalledWith('AuthService.checkAuthentication');
    });
  });
});