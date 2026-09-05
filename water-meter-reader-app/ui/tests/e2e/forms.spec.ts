import { test, expect } from '../fixtures/app-fixtures';

test.describe('Form interactions', () => {
    test('uses required validation on the registration form before submission', async ({ app }) => {
        await app.goToRegister();
        await app.submitCurrentForm('Register');

        const usernameIsValid = await app.page.getByPlaceholder('Username').evaluate((element) =>
            (element as HTMLInputElement).checkValidity(),
        );

        expect(usernameIsValid).toBe(false);
    });

    test('registers a new user and allows that user to log in', async ({ app, mockApi }) => {
        const newUser = {
            username: 'field-user',
            password: 'FreshPassword123!',
            propertyAddress: '12 Lake View Lane',
        };

        await app.register(newUser);
        await app.login({ username: newUser.username, password: newUser.password });
        await app.expectCapturePage();

        expect(mockApi.requestsFor('/api/auth/register')[0]?.body).toEqual(newUser);
        expect(mockApi.requestsFor('/api/auth/login')[0]?.body).toEqual({
            username: newUser.username,
            password: newUser.password,
        });
    });

    test('shows the backend error message when login fails', async ({ app }) => {
        await app.login({ username: 'demo-user', password: 'wrong-password' });
        await app.expectMessage('Invalid username or password.');
    });
});
