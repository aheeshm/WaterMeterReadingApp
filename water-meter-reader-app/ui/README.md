# UI Test Guide

This UI project uses Playwright with TypeScript for end-to-end coverage of the Water Meter Reading App.

## What's Included

- Multi-browser execution for Chromium, Firefox, and WebKit
- Shared Playwright fixtures for app bootstrapping and mocked API setup
- Page object helpers for navigation, authentication, and uploads
- HTML reporting and failure artifacts in `playwright-report/` and `test-results/`

## Run the tests locally

From `water-meter-reader-app/ui`:

```bash
npm ci
npm run playwright:install
npm run test:e2e
```

Useful variants:

```bash
npm run test:e2e:headed
npm run test:e2e:report
```

Playwright 1.63 requires Node.js 20 or newer for the UI test commands.

## Test structure

- `tests/e2e/` - end-to-end specs grouped by feature area
- `tests/fixtures/` - reusable Playwright fixtures
- `tests/page-objects/` - page object models for common UI interactions
- `tests/support/` - mocked backend API setup and shared test state
- `tests/utils/` - helpers for upload files and browser-side fetches

## Common patterns

### Use the shared fixture and page object

```ts
import { test } from '../fixtures/app-fixtures';

test('logs in and opens the capture screen', async ({ app }) => {
  await app.login({ username: 'demo-user', password: 'Password123!' });
  await app.expectCapturePage();
});
```

### Control backend responses for targeted scenarios

```ts
import { createImageFile } from '../utils/testFiles';

test('uploads a reading with a custom rate', async ({ app, mockApi }) => {
  mockApi.setRate(0.08);
  mockApi.queueUpload({ reading: 200 });

  await app.uploadImage(
    createImageFile({ name: 'meter.jpg', mimeType: 'image/jpeg', sizeInBytes: 2048 }),
  );

  await app.expectUsage(200, 16.0);
});
```

## Add a new test

1. Add a new spec under `tests/e2e/` or extend an existing feature file.
2. Reuse `app-fixtures.ts` instead of duplicating setup.
3. Add any repeatable UI actions to `tests/page-objects/waterMeterAppPage.ts`.
4. Add shared API state or response helpers in `tests/support/mockApi.ts`.
5. Run `npm run test:e2e` and inspect `playwright-report/` if a test fails.
