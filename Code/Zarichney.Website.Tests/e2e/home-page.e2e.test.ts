import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should load home page successfully', async ({ page }) => {
    // Navigate to the home page
    await page.goto('/');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check that the page loaded successfully
    await expect(page).toHaveTitle(/Zarichney/i);

    // Verify that key UI elements are present
    await expect(page.locator('header')).toBeVisible();
    
    // Check for logo presence in header
    await expect(page.locator('header app-logo')).toBeVisible();

    // Check for main content area
    await expect(page.locator('main, .main-content, router-outlet')).toBeVisible();

    // Verify that the page is interactive (no loading states blocking interaction)
    await expect(page.locator('body')).not.toHaveClass(/loading/);
  });

  test('should display authentication elements correctly', async ({ page }) => {
    // Navigate to the home page
    await page.goto('/');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check that login button is present for unauthenticated users
    // (This assumes default state is unauthenticated)
    const loginButton = page.locator('a[href="/auth/login"], .login-button');
    await expect(loginButton).toBeVisible();

    // Verify the login button is clickable
    await expect(loginButton).toBeEnabled();
  });

  test('should have working navigation', async ({ page }) => {
    // Navigate to the home page
    await page.goto('/');

    // Wait for the page to load
    await page.waitForLoadState('networkidle');

    // Check that the logo is clickable and leads to home
    const logoLink = page.locator('header a[href="/"], header [routerLink="/"]');
    await expect(logoLink).toBeVisible();

    // Click logo to verify navigation works
    await logoLink.click();
    
    // Verify we're still on home page
    await expect(page).toHaveURL('/');
  });

  test('should be responsive', async ({ page }) => {
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Verify header is still visible on mobile
    await expect(page.locator('header')).toBeVisible();

    // Test desktop viewport
    await page.setViewportSize({ width: 1200, height: 800 });
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Verify layout works on desktop
    await expect(page.locator('header')).toBeVisible();
  });

  test('should handle loading states gracefully', async ({ page }) => {
    // Navigate to the home page
    await page.goto('/');

    // Wait for initial content to load
    await page.waitForSelector('header');

    // Verify no error messages are displayed
    const errorElements = page.locator('.error, [data-testid="error"], .alert-danger');
    await expect(errorElements).toHaveCount(0);

    // Verify no infinite loading spinners
    await page.waitForLoadState('networkidle');
    const loadingSpinners = page.locator('.loading, .spinner, [data-testid="loading"]');
    await expect(loadingSpinners).toHaveCount(0);
  });
});