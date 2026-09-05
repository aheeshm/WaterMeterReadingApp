import { getUserHistory } from './api';

describe('api', () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it('loads and maps backend history when mock mode is disabled', async () => {
        jest.spyOn(global, 'fetch').mockResolvedValue({
            ok: true,
            json: async () => [
                {
                    id: 7,
                    reading: 1450,
                    date: '2026-09-05T12:00:00.000Z',
                },
            ],
        });

        await expect(getUserHistory(12)).resolves.toEqual([
            expect.objectContaining({
                id: 7,
                reading: 1450,
                cost: null,
                uploadedAt: '2026-09-05T12:00:00.000Z',
            }),
        ]);

        expect(global.fetch).toHaveBeenCalledWith('/api/watermeter/readings/12');
    });

    it('falls back to a default message when an error response has no text reader', async () => {
        jest.spyOn(global, 'fetch').mockResolvedValue({
            ok: false,
            json: async () => ({ error: 'bad request' }),
        });

        await expect(getUserHistory(12)).rejects.toThrow('Unable to load history.');
    });
});
