import { test, expect } from '../fixtures/app-fixtures';
import { createImageFile, fetchJson } from '../utils/testFiles';

test.describe('Image uploads, readings, costs, and API integration', () => {
    test.beforeEach(async ({ app }) => {
        await app.login({ username: 'demo-user', password: 'Password123!' });
        await app.expectCapturePage();
    });

    test('requires a file before the upload can be submitted', async ({ app }) => {
        await app.submitUploadWithoutFile();
        await app.expectMessage('Please select a file to upload.');
        await app.expectNoUsageData();
    });

    test('uploads PNG and JPEG files of different sizes and renders the returned readings and costs', async ({ app, mockApi }) => {
        mockApi.queueUpload({ reading: 125 });
        await app.uploadImage(
            createImageFile({
                name: 'meter-reading.png',
                mimeType: 'image/png',
                sizeInBytes: 512,
            }),
        );
        await app.expectUsage(125, 6.25);

        mockApi.setRate(0.08);
        mockApi.queueUpload({ reading: 200 });
        await app.uploadImage(
            createImageFile({
                name: 'meter-reading-large.jpg',
                mimeType: 'image/jpeg',
                sizeInBytes: 50_000,
            }),
        );
        await app.expectUsage(200, 16.0);

        const uploadRequests = mockApi.requestsFor('/api/watermeter/upload');
        expect(uploadRequests).toHaveLength(2);
        expect(uploadRequests[0]?.rawBody).toContain('filename="meter-reading.png"');
        expect(uploadRequests[1]?.rawBody).toContain('filename="meter-reading-large.jpg"');
        expect(uploadRequests[1]?.rawBody).toContain('name="userId"');
    });

    test('rejects uploads when meter validation fails', async ({ app, mockApi }) => {
        mockApi.queueUpload({ reading: 0, errorMessage: 'Please upload a clear image of a water meter.' });

        await app.uploadImage(
            createImageFile({
                name: 'blurry-upload.png',
                mimeType: 'image/png',
                sizeInBytes: 512,
            }),
        );

        await app.expectMessage('Please upload a clear image of a water meter.');
        await app.expectNoUsageData();
    });

    test('rejects readings lower than the previous saved reading', async ({ app, mockApi, page }) => {
        mockApi.queueUpload({ reading: 150 });
        await app.uploadImage(
            createImageFile({
                name: 'first-valid-reading.png',
                mimeType: 'image/png',
                sizeInBytes: 1024,
            }),
        );

        mockApi.queueUpload({ reading: 120 });
        await app.uploadImage(
            createImageFile({
                name: 'older-meter-photo.jpg',
                mimeType: 'image/jpeg',
                sizeInBytes: 2048,
            }),
        );

        await app.expectMessage(
            'Detected reading 120 is lower than the previous reading 150. Upload a current meter image with a reading greater than or equal to the previous month.',
        );

        const readings = await fetchJson<Array<{ reading: number; userId: number; unitId: number }>>(
            page,
            '/api/watermeter/readings/1',
        );

        expect(readings).toHaveLength(1);
        expect(readings[0]).toEqual(expect.objectContaining({ reading: 150, userId: 1, unitId: 1 }));
    });

    test('stores uploaded readings and exposes reading-history and cost endpoints after UI activity', async ({ app, mockApi, page }) => {
        mockApi.queueUpload({ reading: 100 });
        await app.uploadImage(
            createImageFile({
                name: 'first-reading.png',
                mimeType: 'image/png',
                sizeInBytes: 1024,
            }),
        );

        mockApi.setRate(0.07);
        mockApi.queueUpload({ reading: 150 });
        await app.uploadImage(
            createImageFile({
                name: 'second-reading.jpg',
                mimeType: 'image/jpeg',
                sizeInBytes: 2048,
            }),
        );

        const readings = await fetchJson<Array<{ reading: number; userId: number; unitId: number }>>(
            page,
            '/api/watermeter/readings/1',
        );

        expect(readings).toHaveLength(2);
        expect(readings).toEqual([
            expect.objectContaining({ reading: 100, userId: 1, unitId: 1 }),
            expect.objectContaining({ reading: 150, userId: 1, unitId: 1 }),
        ]);

        const costSummary = await fetchJson<{ totalUsage: number; totalCost: number }>(page, '/api/watermeter/cost/1');

        expect(costSummary).toEqual({ totalUsage: 250, totalCost: 15.5 });
    });
});
