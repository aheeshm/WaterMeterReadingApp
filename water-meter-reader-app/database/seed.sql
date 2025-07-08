-- Create Users table
CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    PropertyAddress TEXT NOT NULL
);

-- Add UserId to WaterReadings (if not already present)
-- Note: SQLite does not support ALTER TABLE ADD COLUMN with FOREIGN KEY directly, so you may need to recreate the table for a real migration.
ALTER TABLE WaterReadings ADD COLUMN UserId INTEGER;

-- Sample users (passwords should be hashed in production)
INSERT INTO Users (Username, PasswordHash, PropertyAddress) VALUES ('alice', 'password123', '123 Main St');
INSERT INTO Users (Username, PasswordHash, PropertyAddress) VALUES ('bob', 'password456', '456 Oak Ave');

INSERT INTO RateConfigs (Rate, EffectiveDate) VALUES (0.05, '2023-01-01');
INSERT INTO RateConfigs (Rate, EffectiveDate) VALUES (0.06, '2023-07-01');

INSERT INTO WaterReadings (UnitId, Reading, Date, UserId) VALUES (1, 100, '2023-01-15', 1);
INSERT INTO WaterReadings (UnitId, Reading, Date, UserId) VALUES (1, 150, '2023-02-15', 1);
INSERT INTO WaterReadings (UnitId, Reading, Date, UserId) VALUES (1, 200, '2023-03-15', 1);