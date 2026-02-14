/**
 * ThemeManager - Handles theme switching and persistence
 * Supports light and dark modes with localStorage persistence
 * Respects user's system theme preference on first visit
 */

class ThemeManager {
  constructor() {
    this.storageKey = 'th-mm-pos-theme';
    this.themeAttribute = 'data-theme';
    this.transitionClass = 'theme-transitioning';
    
    // Initialize theme on page load
    this.init();
  }

  /**
   * Initialize the theme system
   * Loads saved theme or detects system preference
   */
  init() {
    // Get saved theme or system preference
    const savedTheme = this.getSavedTheme();
    const systemTheme = this.getSystemTheme();
    const initialTheme = savedTheme || systemTheme;
    
    // Apply theme without transition on initial load
    this.applyTheme(initialTheme, false);
    
    // Listen for system theme changes
    this.watchSystemTheme();
    
    // Set up toggle button listeners
    this.setupToggleButtons();
  }

  /**
   * Get the saved theme from localStorage
   * @returns {string|null} 'light', 'dark', or null if not saved
   */
  getSavedTheme() {
    try {
      return localStorage.getItem(this.storageKey);
    } catch (error) {
      console.warn('Failed to read theme from localStorage:', error);
      return null;
    }
  }

  /**
   * Get the system theme preference
   * @returns {string} 'light' or 'dark'
   */
  getSystemTheme() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
    return 'light';
  }

  /**
   * Get the current active theme
   * @returns {string} 'light' or 'dark'
   */
  getCurrentTheme() {
    return document.documentElement.getAttribute(this.themeAttribute) || 'light';
  }

  /**
   * Apply a theme to the document
   * @param {string} theme - 'light' or 'dark'
   * @param {boolean} withTransition - Whether to animate the transition
   */
  applyTheme(theme, withTransition = true) {
    const root = document.documentElement;
    
    if (withTransition) {
      // Add transitioning class to enable smooth transitions
      root.classList.add(this.transitionClass);
      
      // Remove transitioning class after transition completes
      setTimeout(() => {
        root.classList.remove(this.transitionClass);
      }, 200);
    }
    
    // Set the theme attribute
    root.setAttribute(this.themeAttribute, theme);
    
    // Update toggle button states
    this.updateToggleButtons(theme);
    
    // Dispatch custom event for other components to react
    window.dispatchEvent(new CustomEvent('themechange', { 
      detail: { theme } 
    }));
  }

  /**
   * Save theme preference to localStorage
   * @param {string} theme - 'light' or 'dark'
   */
  saveTheme(theme) {
    try {
      localStorage.setItem(this.storageKey, theme);
    } catch (error) {
      console.warn('Failed to save theme to localStorage:', error);
    }
  }

  /**
   * Toggle between light and dark themes
   */
  toggle() {
    const currentTheme = this.getCurrentTheme();
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    
    this.applyTheme(newTheme, true);
    this.saveTheme(newTheme);
  }

  /**
   * Set a specific theme
   * @param {string} theme - 'light' or 'dark'
   */
  setTheme(theme) {
    if (theme !== 'light' && theme !== 'dark') {
      console.warn(`Invalid theme: ${theme}. Using 'light' instead.`);
      theme = 'light';
    }
    
    this.applyTheme(theme, true);
    this.saveTheme(theme);
  }

  /**
   * Watch for system theme changes and update if no saved preference
   */
  watchSystemTheme() {
    if (!window.matchMedia) return;
    
    const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    // Use addEventListener if available, otherwise use deprecated addListener
    const addListener = darkModeQuery.addEventListener || darkModeQuery.addListener;
    
    if (addListener) {
      addListener.call(darkModeQuery, (e) => {
        // Only auto-switch if user hasn't set a preference
        if (!this.getSavedTheme()) {
          const newTheme = e.matches ? 'dark' : 'light';
          this.applyTheme(newTheme, true);
        }
      });
    }
  }

  /**
   * Set up event listeners for theme toggle buttons
   */
  setupToggleButtons() {
    // Find all theme toggle buttons
    const toggleButtons = document.querySelectorAll('[data-theme-toggle]');
    
    toggleButtons.forEach(button => {
      button.addEventListener('click', (e) => {
        e.preventDefault();
        this.toggle();
      });
    });
  }

  /**
   * Update the visual state of toggle buttons
   * @param {string} theme - Current theme
   */
  updateToggleButtons(theme) {
    const toggleButtons = document.querySelectorAll('[data-theme-toggle]');
    
    toggleButtons.forEach(button => {
      // Update aria-label for accessibility
      const label = theme === 'light' ? 'Switch to dark mode' : 'Switch to light mode';
      button.setAttribute('aria-label', label);
      
      // Update icon visibility if using separate icons
      const lightIcon = button.querySelector('.theme-icon-light');
      const darkIcon = button.querySelector('.theme-icon-dark');
      
      if (lightIcon && darkIcon) {
        if (theme === 'light') {
          lightIcon.style.display = 'none';
          darkIcon.style.display = 'inline-block';
        } else {
          lightIcon.style.display = 'inline-block';
          darkIcon.style.display = 'none';
        }
      }
      
      // Update data attribute
      button.setAttribute('data-current-theme', theme);
    });
  }

  /**
   * Clear saved theme preference (revert to system preference)
   */
  clearSavedTheme() {
    try {
      localStorage.removeItem(this.storageKey);
      const systemTheme = this.getSystemTheme();
      this.applyTheme(systemTheme, true);
    } catch (error) {
      console.warn('Failed to clear theme from localStorage:', error);
    }
  }
}

// Initialize theme manager when DOM is ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => {
    window.themeManager = new ThemeManager();
  });
} else {
  // DOM is already ready
  window.themeManager = new ThemeManager();
}

// Export for module usage if needed
if (typeof module !== 'undefined' && module.exports) {
  module.exports = ThemeManager;
}
