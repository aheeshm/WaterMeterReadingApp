import { expect, type Page } from '@playwright/test';

export type Credentials = {
    username: string;
    password: string;
};

export type RegistrationDetails = Credentials & {
    propertyAddress: string;
};

type UploadFile = {
    name: string;
    mimeType: string;
    buffer: Buffer;
};

export class WaterMeterAppPage {
    readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async goto() {
        await this.page.goto('/');
        await this.page.waitForLoadState('domcontentloaded');
        await expect(this.page.getByRole('heading', { name: 'Water Meter Reader' })).toBeVisible();
    }

    async goToLogin() {
        await this.page.getByRole('navigation').getByRole('button', { name: 'Login' }).click();
        await this.expectLoginPage();
    }

    async goToRegister() {
        await this.page.getByRole('navigation').getByRole('button', { name: 'Register' }).click();
        await this.expectRegisterPage();
    }

    async register(details: RegistrationDetails) {
        await this.goToRegister();
        await this.page.getByPlaceholder('Username').fill(details.username);
        await this.page.getByPlaceholder('Password').fill(details.password);
        await this.page.getByPlaceholder('Property Address').fill(details.propertyAddress);

        await Promise.all([
            this.page.waitForResponse((response) => response.url().includes('/api/auth/register') && response.request().method() === 'POST'),
            this.currentForm().getByRole('button', { name: 'Register' }).click(),
        ]);

        await this.expectLoginPage();
    }

    async login(credentials: Credentials) {
        await this.expectLoginPage();
        await this.page.getByPlaceholder('Username').fill(credentials.username);
        await this.page.getByPlaceholder('Password').fill(credentials.password);

        await Promise.all([
            this.page.waitForResponse((response) => response.url().includes('/api/auth/login') && response.request().method() === 'POST'),
            this.currentForm().getByRole('button', { name: 'Login' }).click(),
        ]);
    }

    async logout() {
        await this.page.getByRole('button', { name: 'Logout' }).click();
        await this.expectLoginPage();
    }

    async submitCurrentForm(buttonName: 'Login' | 'Register') {
        await this.currentForm().getByRole('button', { name: buttonName }).click();
    }

    async submitUploadWithoutFile() {
        await this.currentUploadForm().getByRole('button', { name: 'Upload' }).click();
    }

    async uploadImage(file: UploadFile) {
        await this.page.locator('input[type="file"]').setInputFiles(file);
        await Promise.all([
            this.page.waitForResponse((response) => response.url().includes('/api/watermeter/upload') && response.request().method() === 'POST'),
            this.currentUploadForm().getByRole('button', { name: 'Upload' }).click(),
        ]);
    }

    async expectLoggedOut() {
        const navigation = this.page.getByRole('navigation');
        await expect(navigation.getByRole('button', { name: 'Login' })).toBeVisible();
        await expect(navigation.getByRole('button', { name: 'Register' })).toBeVisible();
    }

    async expectLoginPage() {
        await expect(this.page.getByRole('heading', { name: 'Login' })).toBeVisible();
        await expect(this.currentForm().getByRole('button', { name: 'Login' })).toBeVisible();
    }

    async expectRegisterPage() {
        await expect(this.page.getByRole('heading', { name: 'Register' })).toBeVisible();
        await expect(this.currentForm().getByRole('button', { name: 'Register' })).toBeVisible();
    }

    async expectCapturePage() {
        await expect(this.page.getByRole('heading', { name: 'Upload Water Meter Image' })).toBeVisible();
        await expect(this.page.getByRole('button', { name: 'Capture' })).toBeVisible();
        await expect(this.page.getByRole('button', { name: 'History' })).toBeVisible();
    }

    async expectMessage(message: string) {
        await expect(this.page.getByText(message)).toBeVisible();
    }

    async expectUsage(reading: number, cost: number) {
        await expect(this.page.getByText(`Water Usage: ${reading} gallons`)).toBeVisible();
        await expect(this.page.getByText(`Cost: $${cost.toFixed(2)}`)).toBeVisible();
    }

    async expectNoUsageData() {
        await expect(this.page.getByText('No usage data available.')).toBeVisible();
    }

    private currentForm() {
        return this.page.locator('form').first();
    }

    private currentUploadForm() {
        return this.page.locator('.upload-form form');
    }
}
