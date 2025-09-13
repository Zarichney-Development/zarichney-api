import { Injectable } from '@angular/core';
import { environment } from '../../startup/environments';

interface LogConfig {
    showStackTrace: boolean;
    maxStackDepth: number;
    excludePatterns: RegExp[];
    showTimestamps: boolean;
    logLevel: 'verbose' | 'debug' | 'info' | 'warn' | 'error' | 'none';
}

const APP_SRC_PATTERN = /\/src\/app\//;
const DEFAULT_CONFIG = {
    showStackTrace: true,
    maxStackDepth: 10,
    excludePatterns: [
        /node_modules/,
        /webpack/,
        /zone\.js/,
        /chunk-/,
        /vendor/,
        /core\.mjs/,
        /\.js:\d+/,
        /Observable\.js/,
        /lift\.js/,
        /errorContext\.js/,
    ],
    showTimestamps: true,
    logLevel: 'debug',
} as const satisfies LogConfig;

@Injectable({ providedIn: 'root' })
export class LoggingService {
  private debugMode = !environment.production;
  private config: LogConfig = DEFAULT_CONFIG;
  private readonly isBrowser = typeof window !== 'undefined' && typeof localStorage !== 'undefined';

    constructor() {
        // Check for stored configuration
        this.loadConfig();
        this.info('LoggingService initialized with config:', this.config);
    }

    /**
     * Set logging configuration
     */
    configure(config: Partial<LogConfig>): void {
        this.config = { ...this.config, ...config };
        if (this.isBrowser) {
            try {
                localStorage.setItem('log_config', JSON.stringify(this.config));
            } catch { /* ignore SSR or storage errors */ }
        }
        this.info('LoggingService configuration updated:', this.config);
    }

    /**
     * Load logging configuration from localStorage
     */
    private loadConfig(): void {
        if (!this.isBrowser) return;
        try {
            const savedConfig = localStorage.getItem('log_config');
            if (savedConfig) {
                const parsedConfig = JSON.parse(savedConfig);
                // Convert string patterns back to RegExp
                if (parsedConfig.excludePatterns) {
                    parsedConfig.excludePatterns = parsedConfig.excludePatterns.map(
                        (pattern: string) => new RegExp(pattern.replace(/^\/|\/$/g, ''))
                    );
                }
                this.config = { ...this.config, ...parsedConfig };
            }
        } catch (e) {
            // Use console only in browser to avoid SSR noise
            if (this.isBrowser) {
                console.error('Error loading log configuration:', e);
            }
        }
    }

    /**
     * Debug level message - for detailed troubleshooting
     */
    debug(message: string, ...args: any[]): void {
        if (this.shouldLog('debug')) {
            this.logToConsole('debug', this.getCallerInfo(), message, args);
        }
    }

    /**
     * Info level message - for general operational information
     */
    info(message: string, ...args: any[]): void {
        if (this.shouldLog('info')) {
            this.logToConsole('info', this.getCallerInfo(), message, args);
        }
    }

    /**
     * Error level message - for critical issues
     */
    error(message: string, ...args: any[]): void {
        if (this.shouldLog('error')) {
            this.logToConsole('error', this.getCallerInfo(), message, args);
        }
    }

    /**
     * Verbose level message - for extremely detailed information
     */
    verbose(message: string, ...args: any[]): void {
        if (this.debugMode && this.shouldLog('verbose')) {
            this.logToConsole('debug', this.getCallerInfo(), message, args);
        }
    }

    /**
     * Warning level message - for potential issues
     */
    warn(message: string, ...args: any[]): void {
        if (this.shouldLog('warn')) {
            this.logToConsole('warn', this.getCallerInfo(), message, args);
        }
    }

    /**
     * Log route navigations for debugging
     */
    routeNav(url: string, extra?: any): void {
        if (this.shouldLog('debug')) {
            this.logToConsole('debug', '[Router]', `Navigation to: ${url}`, extra ? [extra] : []);
        }
    }

    /**
     * Check if we should log based on configured level
     */
    private shouldLog(level: string): boolean {
        if (this.config.logLevel === 'none') return false;

        const levels = ['verbose', 'debug', 'info', 'warn', 'error'];
        const configLevelIndex = levels.indexOf(this.config.logLevel);
        const messageLevelIndex = levels.indexOf(level);

        return messageLevelIndex >= configLevelIndex;
    }

    /**
     * Get information about the caller
     */
    private getCallerInfo(): string {
        const error = new Error();
        const stack = error.stack?.split('\n')[3]; // Skip LoggingService frames
        const caller = stack?.match(/at\s+(.*?)\s+\((.*?):\d+:\d+\)/);
        const functionName = caller?.[1] || 'unknown';
        const fileLocation = caller?.[2] || 'unknown';

        // Try to get a more user-friendly file path
        let displayLocation = fileLocation;

        if (APP_SRC_PATTERN.test(fileLocation)) {
            // For app source files, show a cleaner path
            displayLocation = fileLocation.split('/src/app/').pop() || fileLocation;
        } else {
            // For other files, try to get the last segments
            displayLocation = fileLocation.split('/').slice(-2).join('/');
        }

        return `[${functionName} @ ${displayLocation}]`;
    }

    /**
     * Filter stack trace to show only relevant entries
     */
    private filterStackTrace(stack: string): string[] {
        if (!stack) return [];

        const lines = stack.split('\n');
        const filteredLines = lines.filter(line => {
            // Keep the first "Error" line
            if (line.trim().startsWith('Error')) return true;

            // Check against exclude patterns
            return !this.config.excludePatterns.some(pattern => pattern.test(line));
        });

        // Limit stack depth
        return filteredLines.slice(0, this.config.maxStackDepth);
    }

    /**
     * Log to console with formatting
     */
    private logToConsole(level: 'info' | 'warn' | 'error' | 'debug', caller: string, message: string, args: any[]) {
        if (typeof window === 'undefined') return;

        const timestamp = this.config.showTimestamps
            ? `[${new Date().toISOString().split('T')[1].slice(0, -1)}] `
            : '';

        const styles = {
            info: 'background:rgb(130, 199, 255); color: black; padding: 2px 6px; border-radius: 4px; font-weight: bold;',
            error: 'background: #F44336; color: white; padding: 2px 6px; border-radius: 4px; font-weight: bold;',
            warn: 'background: #FF9800; color: black; padding: 2px 6px; border-radius: 4px; font-weight: bold;',
            debug: 'background:rgb(180, 180, 180); color: rgb(61, 61, 61); padding: 2px 6px; border-radius: 4px; font-weight: bold;'
        };

        const prefix = `${timestamp}${level.toUpperCase()} ${caller}`;
        const consoleMethod = level === 'debug' ? 'log' : level;

        console.groupCollapsed(`%c${prefix}`, styles[level]);
        console[consoleMethod](message, ...args);

        if (this.config.showStackTrace) {
            // Get stack trace and filter it
            const error = new Error();
            const filteredStack = this.filterStackTrace(error.stack || '');

            if (filteredStack.length > 0) {
                console.groupCollapsed('App Stack Trace');
                filteredStack.forEach(line => console.log(line));
                console.groupEnd();
            }
        }

        console.groupEnd();
    }
}
