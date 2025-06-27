module.exports = {
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/src/test-setup.ts'],
  testMatch: [
    '<rootDir>/src/**/*.spec.ts',
    '<rootDir>/../Zarichney.Website.Tests/**/*.test.ts'
  ],
  collectCoverageFrom: [
    'src/**/*.ts',
    '!src/**/*.d.ts',
    '!src/test-setup.ts',
    '!src/**/index.ts',
    '!src/**/*.module.ts',
    '!src/startup/**',
    '!src/assets/**'
  ],
  coverageDirectory: '../Zarichney.Website.Tests/coverage',
  coverageReporters: ['html', 'text-summary', 'lcov'],
  transform: {
    '^.+\\.(ts|js|html)$': [
      'jest-preset-angular',
      {
        tsconfig: 'tsconfig.spec.json',
        stringifyContentPathRegex: '\\.html$'
      }
    ]
  },
  transformIgnorePatterns: [
    'node_modules/(?!(@angular|@ngrx|ngx-toastr)/)'
  ],
  moduleFileExtensions: ['ts', 'html', 'js', 'json', 'mjs'],
  testEnvironment: 'jsdom',
  globals: {
    'ts-jest': {
      tsconfig: 'tsconfig.spec.json',
      stringifyContentPathRegex: '\\.html$'
    }
  }
};