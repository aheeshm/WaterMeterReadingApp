import { test as base, expect } from '@playwright/test';
import { WaterMeterAppPage } from '../page-objects/waterMeterAppPage';
import { createMockApi, type MockApiController } from '../support/mockApi';

type AppFixtures = {
    app: WaterMeterAppPage;
    mockApi: MockApiController;
};

export const test = base.extend<AppFixtures>({
    mockApi: async ({ page }, use) => {
        const mockApi = await createMockApi(page);
        await use(mockApi);
    },
    app: async ({ page, mockApi }, use) => {
        const app = new WaterMeterAppPage(page);
        await app.goto();
        await use(app);
    },
});

export { expect };
