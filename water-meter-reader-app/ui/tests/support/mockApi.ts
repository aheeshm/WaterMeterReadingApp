import type { Page } from '@playwright/test';

export type RequestLog = {
    method: string;
    url: string;
    body?: unknown;
    rawBody?: string;
};

type UserRecord = {
    userId: number;
    username: string;
    password: string;
    propertyAddress: string;
};

type ReadingRecord = {
    id: number;
    userId: number;
    unitId: number;
    reading: number;
    date: string;
    cost: number;
    rate: number;
    fileName: string;
};

type UploadPlan = {
    reading: number;
};

export type MockApiController = {
    readonly state: {
        activeRate: number;
        users: UserRecord[];
        readings: ReadingRecord[];
        requests: RequestLog[];
    };
    setRate(rate: number): void;
    queueUpload(plan: UploadPlan): void;
    requestsFor(pathname: string): RequestLog[];
};

type AuthPayload = {
    username: string;
    password: string;
    propertyAddress?: string;
};

const defaultUsers: UserRecord[] = [
    {
        userId: 1,
        username: 'demo-user',
        password: 'Password123!',
        propertyAddress: '100 River Road',
    },
];

export async function createMockApi(page: Page): Promise<MockApiController> {
    let nextUserId = defaultUsers.length + 1;
    let nextReadingId = 1;
    let activeRate = 0.05;
    const queuedUploads: UploadPlan[] = [];
    const users = defaultUsers.map((user) => ({ ...user }));
    const readings: ReadingRecord[] = [];
    const requests: RequestLog[] = [];

    const controller: MockApiController = {
        state: {
            get activeRate() {
                return activeRate;
            },
            users,
            readings,
            requests,
        },
        setRate(rate: number) {
            activeRate = rate;
        },
        queueUpload(plan: UploadPlan) {
            queuedUploads.push(plan);
        },
        requestsFor(pathname: string) {
            return requests.filter((request) => request.url.endsWith(pathname));
        },
    };

    await page.route('**/api/auth/register', async (route) => {
        const payload = route.request().postDataJSON() as AuthPayload;
        requests.push({
            method: route.request().method(),
            url: new URL(route.request().url()).pathname,
            body: payload,
        });

        const userExists = users.some((user) => user.username === payload.username);
        if (userExists) {
            await route.fulfill({ status: 400, body: 'Username already exists.' });
            return;
        }

        users.push({
            userId: nextUserId++,
            username: payload.username,
            password: payload.password,
            propertyAddress: payload.propertyAddress ?? '',
        });

        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ message: 'Registration successful.' }),
        });
    });

    await page.route('**/api/auth/login', async (route) => {
        const payload = route.request().postDataJSON() as AuthPayload;
        requests.push({
            method: route.request().method(),
            url: new URL(route.request().url()).pathname,
            body: payload,
        });

        const user = users.find(
            (candidate) => candidate.username === payload.username && candidate.password === payload.password,
        );

        if (!user) {
            await route.fulfill({ status: 401, body: 'Invalid username or password.' });
            return;
        }

        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
                userId: user.userId,
                username: user.username,
                address: user.propertyAddress,
            }),
        });
    });

    await page.route('**/api/watermeter/upload', async (route) => {
        const request = route.request();
        const rawBody = request.postDataBuffer()?.toString('latin1') ?? '';
        const userId = Number(rawBody.match(/name="userId"\r\n\r\n(\d+)/)?.[1] ?? 0);
        const fileName = rawBody.match(/filename="([^"]+)"/)?.[1] ?? 'upload-image';

        requests.push({
            method: request.method(),
            url: new URL(request.url()).pathname,
            rawBody,
        });

        if (!rawBody.includes('filename="') || userId === 0) {
            await route.fulfill({ status: 400, body: 'No file uploaded.' });
            return;
        }

        const plannedUpload = queuedUploads.shift();
        const reading = plannedUpload?.reading ?? 100;
        const date = new Date('2026-01-01T00:00:00.000Z').toISOString();
        const cost = Number((reading * activeRate).toFixed(2));

        readings.push({
            id: nextReadingId++,
            userId,
            unitId: 1,
            reading,
            date,
            cost,
            rate: activeRate,
            fileName,
        });

        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({
                reading,
                cost,
                date,
                userId,
            }),
        });
    });

    await page.route('**/api/watermeter/readings/*', async (route) => {
        const pathname = new URL(route.request().url()).pathname;
        const userId = Number(pathname.split('/').pop());
        requests.push({
            method: route.request().method(),
            url: pathname,
        });

        const matchingReadings = readings.filter((reading) => reading.userId === userId);
        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify(matchingReadings),
        });
    });

    await page.route('**/api/watermeter/cost/*', async (route) => {
        const pathname = new URL(route.request().url()).pathname;
        const unitId = Number(pathname.split('/').pop());
        requests.push({
            method: route.request().method(),
            url: pathname,
        });

        const matchingReadings = readings.filter((reading) => reading.unitId === unitId);
        if (!matchingReadings.length) {
            await route.fulfill({ status: 404, body: 'No readings found for the specified unit.' });
            return;
        }

        const totalUsage = matchingReadings.reduce((sum, reading) => sum + reading.reading, 0);
        const totalCost = Number(matchingReadings.reduce((sum, reading) => sum + reading.cost, 0).toFixed(2));

        await route.fulfill({
            status: 200,
            contentType: 'application/json',
            body: JSON.stringify({ totalUsage, totalCost }),
        });
    });

    return controller;
}
