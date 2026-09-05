import { defineConfig, devices } from '@playwright/test';

const port = process.env.PORT ?? '3000';
const baseURL = process.env.PLAYWRIGHT_BASE_URL ?? `http://127.0.0.1:${port}`;
const nodeMajorVersion = Number.parseInt(process.versions.node.split('.')[0], 10);
const needsLegacyOpenSslProvider = nodeMajorVersion >= 21;

export default defineConfig({
    testDir: './tests/e2e',
    timeout: 30 * 1000,
    expect: {
        timeout: 5 * 1000,
    },
    fullyParallel: true,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 2 : 0,
    workers: process.env.CI ? 1 : undefined,
    outputDir: 'test-results',
    reporter: [
        ['list'],
        ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ],
    use: {
        baseURL,
        trace: 'retain-on-failure',
        screenshot: 'only-on-failure',
        video: 'retain-on-failure',
        actionTimeout: 10 * 1000,
    },
    webServer: {
        command: 'node node_modules/react-scripts/bin/react-scripts.js start',
        cwd: __dirname,
        env: {
            ...process.env,
            BROWSER: 'none',
            ...(needsLegacyOpenSslProvider
                ? {
                    NODE_OPTIONS: [process.env.NODE_OPTIONS, '--openssl-legacy-provider'].filter(Boolean).join(' '),
                }
                : {}),
        },
        url: baseURL,
        reuseExistingServer: !process.env.CI,
        timeout: 120 * 1000,
    },
    projects: [
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
        },
        {
            name: 'firefox',
            use: { ...devices['Desktop Firefox'] },
        },
        {
            name: 'webkit',
            use: { ...devices['Desktop Safari'] },
        },
    ],
});
