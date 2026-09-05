import { test } from '../fixtures/app-fixtures';

test.describe('UI navigation', () => {
    test('loads the login screen by default and lets users switch to registration and back', async ({ app }) => {
        await app.expectLoggedOut();
        await app.expectLoginPage();

        await app.goToRegister();
        await app.goToLogin();
    });

    test('shows authenticated navigation after login and returns to login after logout', async ({ app }) => {
        await app.login({ username: 'demo-user', password: 'Password123!' });
        await app.expectCapturePage();

        await app.logout();
        await app.expectLoggedOut();
    });
});
