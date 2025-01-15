CREATE TABLE Users (
    id TEXT PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE TABLE Categories (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    isDefault BOOLEAN NOT NULL DEFAULT 0
);

INSERT INTO Categories (id, name, isDefault) VALUES
    ('00000000-0000-0000-0000-000000000000', 'Housing', 1),
    ('00000000-0000-0000-0000-000000000001', 'Transportation', 1),
    ('00000000-0000-0000-0000-000000000002', 'Food', 1),
    ('00000000-0000-0000-0000-000000000003', 'Entertainment', 1),
    ('00000000-0000-0000-0000-000000000004', 'Healthcare', 1);

CREATE TABLE Expenses (
    id TEXT PRIMARY KEY,
    amount REAL NOT NULL,
    description TEXT NOT NULL,
    categoryId TEXT NOT NULL,
    userId TEXT NOT NULL,
    label TEXT CHECK(label IN ('daily', 'weekly', 'semi-monthly', 'monthly')) NOT NULL,
    date TEXT NOT NULL,
    FOREIGN KEY (categoryId) REFERENCES Categories(id),
    FOREIGN KEY (userId) REFERENCES Users(id)
);