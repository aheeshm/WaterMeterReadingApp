const DEMO_USERS_KEY = 'water-meter-reader-demo-users';
const DEMO_HISTORY_KEY = 'water-meter-reader-demo-history';
const DEFAULT_DEMO_USER = {
    userId: 'demo-user',
    username: 'demo',
    password: 'demo123',
    propertyAddress: '123 Demo Lane',
};

const getStorage = () => {
    if (typeof window === 'undefined' || !window.localStorage) {
        return null;
    }

    return window.localStorage;
};

const readCollection = (key) => {
    const storage = getStorage();

    if (!storage) {
        return [];
    }

    try {
        const value = storage.getItem(key);
        return value ? JSON.parse(value) : [];
    } catch (error) {
        return [];
    }
};

const writeCollection = (key, items) => {
    const storage = getStorage();

    if (!storage) {
        return;
    }

    storage.setItem(key, JSON.stringify(items));
};

const getUsers = () => {
    const users = readCollection(DEMO_USERS_KEY);
    const hasDefaultUser = users.some((user) => user.username === DEFAULT_DEMO_USER.username);

    if (hasDefaultUser) {
        return users;
    }

    return [DEFAULT_DEMO_USER, ...users];
};

const calculateReading = (file) => {
    const baseReading = 1200;
    const sizeBonus = file && typeof file.size === 'number' ? file.size % 700 : 175;
    return baseReading + sizeBonus;
};

export const isMockMode = () => {
    if (process.env.REACT_APP_USE_MOCK_API === 'true') {
        return true;
    }

    return typeof window !== 'undefined' && /github\.io$/i.test(window.location.hostname);
};

export const loginDemoUser = async ({ username, password }) => {
    const users = getUsers();
    const user = users.find((candidate) => candidate.username === username && candidate.password === password);

    if (!user) {
        throw new Error('Invalid demo credentials. Use demo / demo123 or register a new demo user.');
    }

    return {
        userId: user.userId,
        username: user.username,
        propertyAddress: user.propertyAddress,
    };
};

export const registerDemoUser = async ({ username, password, propertyAddress }) => {
    const normalizedUsername = username.trim();
    const users = getUsers();

    if (users.some((candidate) => candidate.username.toLowerCase() === normalizedUsername.toLowerCase())) {
        throw new Error('That demo username is already in use. Please choose another one.');
    }

    const nextUser = {
        userId: `demo-${Date.now()}`,
        username: normalizedUsername,
        password,
        propertyAddress: propertyAddress.trim(),
    };

    writeCollection(DEMO_USERS_KEY, [...users, nextUser]);

    return {
        message: 'Registration successful! Sign in with your demo credentials to continue.',
    };
};

export const uploadDemoReading = async ({ file, userId }) => {
    if (!file) {
        throw new Error('Please select a file to upload.');
    }

    const reading = calculateReading(file);
    const cost = Number((reading * 0.0042).toFixed(2));
    const uploadRecord = {
        id: `${userId}-${Date.now()}`,
        userId,
        fileName: file.name,
        reading,
        cost,
        uploadedAt: new Date().toISOString(),
    };
    const history = readCollection(DEMO_HISTORY_KEY);

    writeCollection(DEMO_HISTORY_KEY, [uploadRecord, ...history]);

    return {
        ...uploadRecord,
        message: 'Demo upload complete. This reading is mocked for the static GitHub Pages preview.',
    };
};

export const getDemoHistory = async (userId) => {
    return readCollection(DEMO_HISTORY_KEY).filter((entry) => entry.userId === userId);
};

export const __resetDemoState = () => {
    const storage = getStorage();

    if (!storage) {
        return;
    }

    storage.removeItem(DEMO_USERS_KEY);
    storage.removeItem(DEMO_HISTORY_KEY);
};
