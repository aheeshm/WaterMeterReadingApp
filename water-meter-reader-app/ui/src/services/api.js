import {
    getDemoHistory,
    isMockMode,
    loginDemoUser,
    registerDemoUser,
    uploadDemoReading,
} from './mockApi';

const readErrorMessage = async (response, fallbackMessage) => {
    const text = await response.text();
    return text || fallbackMessage;
};

export const loginUser = async (credentials) => {
    if (isMockMode()) {
        return loginDemoUser(credentials);
    }

    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(credentials),
    });

    if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Login failed.'));
    }

    return response.json();
};

export const registerUser = async (payload) => {
    if (isMockMode()) {
        return registerDemoUser(payload);
    }

    const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
    });

    if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Registration failed.'));
    }

    return {
        message: 'Registration successful! You can now log in.',
    };
};

export const uploadWaterMeterImage = async ({ file, userId }) => {
    if (isMockMode()) {
        return uploadDemoReading({ file, userId });
    }

    const formData = new FormData();
    formData.append('file', file);
    formData.append('userId', userId);

    const response = await fetch('/api/watermeter/upload', {
        method: 'POST',
        body: formData,
    });

    if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Upload failed. Please try again.'));
    }

    return response.json();
};

export const getUserHistory = async (userId) => {
    if (isMockMode()) {
        return getDemoHistory(userId);
    }

    const response = await fetch(`/api/watermeter/readings/${encodeURIComponent(userId)}`);

    if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Unable to load history.'));
    }

    const readings = await response.json();

    return readings.map((entry) => ({
        id: entry.id,
        fileName: entry.date ? `Reading from ${new Date(entry.date).toLocaleString()}` : 'Saved reading',
        reading: entry.reading,
        cost: typeof entry.cost === 'number' ? entry.cost : null,
        uploadedAt: entry.date,
    }));
};

export { isMockMode };
