# Water Meter Reader Application

This project is a comprehensive application designed to facilitate the uploading, analysis, and management of water meter readings. It consists of three main components: an API, a database, and a user interface (UI).

## Project Structure

- **api**: Contains the backend services, including controllers, models, services, and the database context.
  - **Controllers**: Handles HTTP requests related to water meter readings.
  - **Models**: Defines the data structures for water readings and rate configurations.
  - **Services**: Contains logic for analyzing images of water meters.
  - **Data**: Manages the database context for Entity Framework Core.
  
- **database**: Contains database migration files and seed scripts for initializing data.
  
- **ui**: The frontend application built with React, allowing users to interact with the API.
  - **public**: Contains the main HTML file for the UI.
  - **src**: Contains the React components and styles.

## Features

- Upload images of water meters for analysis.
- Store water meter readings in a database.
- Calculate water usage and costs based on configurable rates.
- User-friendly interface for managing readings and viewing costs.

## Setup Instructions

1. **Clone the repository**:
   ```
   git clone <repository-url>
   cd water-meter-reader-app
   ```

2. **Set up the database**:
   - Navigate to the `database` directory and run the seed script to initialize the database with default values.

3. **Run the API**:
   - Navigate to the `api` directory and run the application using your preferred method (e.g., using `dotnet run`).

4. **Run the UI**:
   - Navigate to the `ui` directory and install dependencies:
     ```
     npm install
     ```
   - Start the UI application:
     ```
     npm start
     ```

## UI Testing

The UI now includes a Playwright end-to-end test framework with TypeScript support.

- Playwright setup, fixtures, page objects, and specs live in `ui/`
- Local test instructions and examples are documented in `/home/runner/work/WaterMeterReadingApp/WaterMeterReadingApp/water-meter-reader-app/ui/README.md`

## Usage

- Access the UI in your web browser at `http://localhost:3000`.
- Use the upload form to submit images of water meters.
- View calculated water usage and costs based on the readings.

## Contributing

Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.