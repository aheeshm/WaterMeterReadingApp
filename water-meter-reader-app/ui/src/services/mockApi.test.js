import {
    __resetDemoState,
    getDemoHistory,
    loginDemoUser,
    registerDemoUser,
    uploadDemoReading,
} from './mockApi';

describe('mockApi', () => {
    beforeEach(() => {
        __resetDemoState();
    });

    it('registers and logs in a demo user', async () => {
        await registerDemoUser({
            username: 'alice',
            password: 'password123',
            propertyAddress: '42 Main Street',
        });

        await expect(loginDemoUser({
            username: 'alice',
            password: 'password123',
        })).resolves.toEqual(expect.objectContaining({
            username: 'alice',
            propertyAddress: '42 Main Street',
        }));
    });

    it('stores uploaded demo readings in browser history', async () => {
        const upload = await uploadDemoReading({
            file: new File(['meter-image'], 'meter.png', { type: 'image/png' }),
            userId: 'demo-user',
        });

        expect(upload.reading).toBeGreaterThan(0);
        expect(upload.cost).toBeGreaterThan(0);

        await expect(getDemoHistory('demo-user')).resolves.toEqual([
            expect.objectContaining({
                fileName: 'meter.png',
                reading: upload.reading,
                cost: upload.cost,
            }),
        ]);
    });
});
