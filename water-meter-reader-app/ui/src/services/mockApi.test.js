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

    it('creates unique identifiers even when registrations and uploads share a timestamp', async () => {
        const nowSpy = jest.spyOn(Date, 'now').mockReturnValue(1725566400000);

        try {
            await registerDemoUser({
                username: 'alice',
                password: 'password123',
                propertyAddress: '42 Main Street',
            });
            await registerDemoUser({
                username: 'bob',
                password: 'password123',
                propertyAddress: '43 Main Street',
            });

            const alice = await loginDemoUser({
                username: 'alice',
                password: 'password123',
            });
            const bob = await loginDemoUser({
                username: 'bob',
                password: 'password123',
            });

            const firstUpload = await uploadDemoReading({
                file: new File(['one'], 'one.png', { type: 'image/png' }),
                userId: alice.userId,
            });
            const secondUpload = await uploadDemoReading({
                file: new File(['two'], 'two.png', { type: 'image/png' }),
                userId: alice.userId,
            });

            expect(alice.userId).not.toBe(bob.userId);
            expect(firstUpload.id).not.toBe(secondUpload.id);
        } finally {
            nowSpy.mockRestore();
        }
    });
});
