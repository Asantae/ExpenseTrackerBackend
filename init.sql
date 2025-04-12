CREATE TABLE IF NOT EXISTS Users (
    id TEXT PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE IF NOT EXISTS Categories (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    isDefault BOOLEAN NOT NULL DEFAULT 0,
    createdBy TEXT NULL,
    FOREIGN KEY (createdBy) REFERENCES Users(id)
);

INSERT INTO Categories (id, name, isDefault, createdBy) VALUES
    ('00000000-0000-0000-0000-000000000000', 'Housing', 1, null),
    ('00000000-0000-0000-0000-000000000001', 'Transportation', 1, null),
    ('00000000-0000-0000-0000-000000000002', 'Food', 1, null),
    ('00000000-0000-0000-0000-000000000003', 'Entertainment', 1, null),
    ('00000000-0000-0000-0000-000000000004', 'Healthcare', 1, null);

CREATE TABLE IF NOT EXISTS Frequency (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

INSERT INTO Frequency (id, name) VALUES
    (0, 'One-Time'),
    (1, 'Daily'),
    (2, 'Weekly'),
    (3, 'Semi-Monthly'),
    (4, 'Monthly');

CREATE TABLE IF NOT EXISTS Expenses (
    id TEXT PRIMARY KEY,
    amount REAL NOT NULL,
    description TEXT NOT NULL,
    categoryId TEXT NOT NULL,
    userId TEXT NOT NULL,
    frequencyId INTEGER NOT NULL,
    createdDate TEXT NOT NULL,
    FOREIGN KEY (categoryId) REFERENCES Categories(id),
    FOREIGN KEY (userId) REFERENCES Users(id),
    FOREIGN KEY (frequencyId) REFERENCES Frequency(id)
);

CREATE TABLE IF NOT EXISTS RefreshTokens (
    id TEXT PRIMARY KEY,
    userId TEXT NOT NULL,
    token TEXT NOT NULL UNIQUE,
    expiresAt DATETIME NOT NULL,
    isRevoked BOOLEAN NOT NULL DEFAULT 0
);