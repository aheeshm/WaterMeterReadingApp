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
            file: new File(['x'.repeat(64)], 'meter.png', { type: 'image/png' }),
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

    it('rejects uploads that are not valid meter images', async () => {
        await expect(uploadDemoReading({
            file: new File(['short'], 'notes.txt', { type: 'text/plain' }),
            userId: 'demo-user',
        })).rejects.toThrow('Please upload a clear image of a water meter.');
    });

    it('rejects readings lower than the previous saved reading and allows equal readings', async () => {
        await uploadDemoReading({
            file: new File(['x'.repeat(699)], 'meter-1.png', { type: 'image/png' }),
            userId: 'demo-user',
        });

        await expect(uploadDemoReading({
            file: new File(['x'.repeat(32)], 'meter-2.png', { type: 'image/png' }),
            userId: 'demo-user',
        })).rejects.toThrow('greater than or equal to the previous month');

        await expect(uploadDemoReading({
            file: new File(['x'.repeat(699)], 'meter-3.png', { type: 'image/png' }),
            userId: 'demo-user',
        })).resolves.toEqual(expect.objectContaining({
            fileName: 'meter-3.png',
        }));
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
                file: new File(['x'.repeat(64)], 'one.png', { type: 'image/png' }),
                userId: alice.userId,
            });
            const secondUpload = await uploadDemoReading({
                file: new File(['x'.repeat(64)], 'two.png', { type: 'image/png' }),
                userId: alice.userId,
            });

            expect(alice.userId).not.toBe(bob.userId);
            expect(firstUpload.id).not.toBe(secondUpload.id);
        } finally {
            nowSpy.mockRestore();
        }
    });
});
