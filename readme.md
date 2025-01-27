Expense Tracker Backend
This is the backend API for the Expense Tracker project. It is built using ASP.NET Core and SQLite for managing expense data. The API is designed to interact with a frontend application, supporting various functionalities such as expense management, category management, and user management.

Features
RESTful API endpoints for managing expense data, categories, and users.
SQLite database integration.
Support for CRUD operations on expenses and categories.
User authentication setup for future login feature.
Ability to assign categories and link expenses to users.
Use of GUIDs for unique identifiers.
Environment configuration using a .env file for sensitive data like connection strings.
Prerequisites
.NET SDK (6.0 or later)
SQLite installed on your system.
A valid SQLite database file (expense_tracker.db).
dotenv or any environment configuration library for managing connection strings.
Project Structure
bash
Copy code
ExpenseTrackerBackend/
├── Controllers/
│   ├── AuthController.cs
│   ├── ExpenseController.cs
│   └── UserController.cs
├── Data/
│   ├── ExpenseRepository.cs
│   └── UserRepository.cs
├── dbFiles/
│   └── initialization_schema.sql
├── Enums/
│   ├── CategoryEnum.cs
│   └── FrequencyEnum.cs
├── Models/
│   ├── Category.cs
│   ├── Expense.cs
│   ├── RefreshTokenRequest.cs
│   ├── User.cs
│   ├── UserLoginRequest.cs
│   └── UserRegistrationRequest.cs
├── Utilities/
│   ├── ExpensesUtility.cs
│   ├── JwtTokenUtility.cs
│   ├── PasswordUtility.cs
│   └── UserUtility.cs
├── expense_tracker.db
├── Program.cs
├── appsettings.json
├── .env
└── ExpenseTrackerBackend.csproj
Setup Instructions
1. Clone the Repository
After creating a GitHub repository, clone it to your local system:

bash
Copy code
git clone https://github.com/Asantae/ExpenseTrackerBackend.git
cd ExpenseTrackerBackend
2. Add the SQLite Database
Ensure the expense_tracker.db file is located in the Data folder. If you don’t have the database, run the update_schema.sql script found in the dbUpdater folder to set up the necessary tables.

3. Set Up the Connection String
In the .appsettings.json file, add the following line to specify the database connection string:

makefile
Copy code
CONNECTION_STRING=Data Source=expense_tracker.db;Version=3;
In the Program.cs file, make sure the connection string is pulled from the environment variable:

csharp
Copy code
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
4. Run the Application
Restore the packages, build the project, and run the application:

bash
Copy code
dotnet restore
dotnet build
dotnet run
By default, the backend will run on http://localhost:5221.

5. Test the API
You can use tools like Postman or Curl to test the endpoints. Example:

bash
Copy code
curl http://localhost:5221/api/expenses/categories
Endpoints
Method	Endpoint	Description
GET	/api/expenses/categories	Retrieves expense categories
GET	/api/expenses	Retrieves all expenses
POST	/api/expenses	Adds a new expense
PUT	/api/expenses/{id}	Updates an expense
DELETE	/api/expenses/{id}	Deletes an expense
GET	/api/expenses/users/{userId}	Retrieves expenses for a specific user
Database Schema Changes
The backend uses three models: Expense, Category, and User. These models are linked via foreign keys, and the Expense table has a reference to the Category and User tables.

Users: Stores user information (ID, Username, Email, Password, CreatedAt).
Categories: Stores expense categories (ID, Name).
Expenses: Stores expense data, with references to both categories and users (ID, Amount, Description, CategoryId, UserId, Label, Date).
Additional Notes
Ensure the .env file is added to your .gitignore to avoid exposing sensitive data like the connection string.
Future features like user authentication will be implemented using JWT tokens to secure API endpoints.
The expense labels include options for daily, weekly, semi-monthly, and monthly.