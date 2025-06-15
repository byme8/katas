import { signal } from '@angular/core';

// Storage key for persistence
const STORAGE_KEY = 'pagination-app-settings';

// Default settings
const DEFAULT_SKIP_COUNT_CALCULATION = false;

// Load initial value from localStorage
function loadSkipCountCalculation(): boolean {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored);
      return parsed.skipCountCalculation ?? DEFAULT_SKIP_COUNT_CALCULATION;
    }
  } catch (error) {
    console.warn('Failed to load settings from localStorage:', error);
  }
  return DEFAULT_SKIP_COUNT_CALCULATION;
}

// Save to localStorage
function saveSkipCountCalculation(value: boolean): void {
  try {
    const settings = { skipCountCalculation: value };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
  } catch (error) {
    console.warn('Failed to save settings to localStorage:', error);
  }
}

// Global signal for skip count calculation setting
export const skipCountCalculation = signal(loadSkipCountCalculation());


// Helper functions for better ergonomics
export const toggleSkipCountCalculation = () => {
  const newValue = !skipCountCalculation();
  skipCountCalculation.set(newValue);
  saveSkipCountCalculation(newValue);
};

export const setSkipCountCalculation = (value: boolean) => {
  skipCountCalculation.set(value);
  saveSkipCountCalculation(value);
};

export const resetSkipCountCalculation = () => {
  skipCountCalculation.set(DEFAULT_SKIP_COUNT_CALCULATION);
  saveSkipCountCalculation(DEFAULT_SKIP_COUNT_CALCULATION);
};

// Export for read-only access
export const isSkipCountCalculationEnabled = skipCountCalculation.asReadonly();