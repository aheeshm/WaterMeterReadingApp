# GitHub Pages Setup

## Hosted URL

- Default GitHub Pages URL: https://aheeshm.github.io/WaterMeterReadingApp/
- Optional custom domain: set the `GITHUB_PAGES_CUSTOM_DOMAIN` repository variable and add the matching DNS record before redeploying.

## One-time repository setup

1. Open **Settings** → **Pages** for this repository.
2. Set **Source** to **GitHub Actions**.
3. Save the settings.

After that, every push to `main` will rebuild the React frontend and publish the static site through GitHub Actions.

## Redeploy instructions

Anyone with push access can redeploy by:

1. Pushing a change to `main`, or
2. Opening **Actions** → **Deploy React UI to GitHub Pages** → **Run workflow**

The workflow:

- installs dependencies with `npm ci`
- restores the npm cache through `actions/setup-node`
- builds the React app with `npm run build:pages`
- uploads the generated static files as a GitHub Pages artifact
- deploys the artifact to GitHub Pages with the official Pages actions

## Mocked demo behavior

The GitHub Pages build runs with `REACT_APP_USE_MOCK_API=true`, so the live demo works without the API or database.

Mocked features:

- login supports the built-in demo account `demo / demo123`
- register stores demo users in browser `localStorage`
- upload generates a mocked water reading and cost from the selected file
- history is limited to uploads saved in the current browser

Not available in the static demo:

- real backend authentication
- server-side data persistence
- actual image analysis from the .NET API
