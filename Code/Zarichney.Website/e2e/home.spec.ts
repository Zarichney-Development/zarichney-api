import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should load and display main navigation', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page loads
    await expect(page).toHaveTitle(/Zarichney/);
    
    // Check for main navigation elements
    await expect(page.locator('app-header')).toBeVisible();
  });
  
  test('should have working responsive menu', async ({ page }) => {
    await page.goto('/');
    
    // Should have a menu component
    const menu = page.locator('app-menu');
    await expect(menu).toBeAttached();
  });
});