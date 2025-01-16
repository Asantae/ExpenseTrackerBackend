CREATE TABLE Users (
    id TEXT PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE Categories (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    isDefault BOOLEAN NOT NULL DEFAULT 0,
    createdBy TEXT NULL,
    FOREIGN KEY (createdBy) REFERENCES Users(id)
);

INSERT INTO Categories (id, name, isDefault) VALUES
    (1, 'Housing', 1),
    (2, 'Transportation', 1),
    (3, 'Food', 1),
    (4, 'Entertainment', 1),
    (5, 'Healthcare', 1);

CREATE TABLE Frequency (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

INSERT INTO Frequency (id, name) VALUES
    (0, 'Daily'),
    (1, 'Weekly'),
    (2, 'Semi-Monthly'),
    (3, 'Monthly');

CREATE TABLE Expenses (
    id TEXT PRIMARY KEY,
    amount REAL NOT NULL,
    description TEXT NOT NULL,
    categoryId INTEGER NOT NULL,
    userId TEXT NOT NULL,
    frequencyId INTEGER NOT NULL,
    date TEXT NOT NULL,
    FOREIGN KEY (categoryId) REFERENCES Categories(id),
    FOREIGN KEY (userId) REFERENCES Users(id),
    FOREIGN KEY (frequencyId) REFERENCES Frequency(id)
);
