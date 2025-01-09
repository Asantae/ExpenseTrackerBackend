Expense Tracker Backend

This is the backend API for the Expense Tracker project. It is built using ASP.NET Core and SQLite for managing expense data.

Features

RESTful API endpoints for managing expense data and categories.

SQLite database integration.

Designed to interact with a frontend application.

Prerequisites

.NET SDK (6.0 or later)

SQLite installed on your system.

A valid SQLite database file (expense_tracker.db).

Project Structure

ExpenseTrackerBackend/
├── Controllers/
│   └── ExpensesController.cs
├── Data/
│   └── expense_tracker.db
├── Program.cs
├── appsettings.json
└── ExpenseTrackerBackend.csproj

Setup Instructions

1. Clone the Repository

After creating a GitHub repository, clone it to your local system:

git clone <your-repo-url>
cd ExpenseTrackerBackend

2. Add the SQLite Database

Ensure the expense_tracker.db file is located in the appropriate folder. Update the connectionString in Program.cs or appsettings.json if needed:

"ConnectionStrings": {
  "DefaultConnection": "Data Source=expense_tracker.db;Version=3;"
}

3. Run the Application

Restore the packages, build the project, and run the application:

dotnet restore
dotnet build
dotnet run

By default, the backend will run on http://localhost:5221.

4. Test the API

You can use tools like Postman or Curl to test the endpoints. Example:

curl http://localhost:5221/api/expenses/categories

Endpoints

Method

Endpoint

Description

GET

/api/expenses/categories

Retrieves expense categories

GET

/api/expenses

Retrieves all expenses

POST

/api/expenses

Adds a new expense

PUT

/api/expenses/{id}

Updates an expense

DELETE

/api/expenses/{id}

Deletes an expense