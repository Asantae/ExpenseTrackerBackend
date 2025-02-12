using ExpenseTrackerBackend.Enums;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Dtos;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackerBackend.Repositories
{
    public class ExpenseRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ExpenseRepository> _logger;

        public ExpenseRepository(string connectionString, ILogger<ExpenseRepository> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public void AddExpense(Expense expense)
        {
            _logger.LogInformation("AddExpense called for ExpenseId: {ExpenseId}, CreatedBy: {CreatedBy}", expense.Id, expense.CreatedBy);
            int frequencyId = (int)expense.Frequency;
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Expenses (id, userId, amount, description, categoryId, frequencyId, createdDate)
                VALUES (@id, @userId, @amount, @description, @categoryId, @frequencyId, @createdDate)";
            command.Parameters.AddWithValue("@id", expense.Id.ToString());
            command.Parameters.AddWithValue("@userId", expense.CreatedBy.ToString());
            command.Parameters.AddWithValue("@amount", expense.Amount.ToString());
            command.Parameters.AddWithValue("@description", expense.Description.ToString());
            command.Parameters.AddWithValue("@categoryId", expense.CategoryId.ToString());
            command.Parameters.AddWithValue("@frequencyId", frequencyId);
            command.Parameters.AddWithValue("@createdDate", expense.Date);

            command.ExecuteNonQuery();
        }

        public void UpdateExpense(Expense expense)
        {
            _logger.LogInformation("UpdateExpense called for ExpenseId: {ExpenseId}", expense.Id);
            int frequencyId = (int)expense.Frequency;
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Expenses
                SET 
                    amount = @amount,
                    description = @description,
                    categoryId = @categoryId,
                    frequencyId = @frequencyId
                WHERE id = @id";

            command.Parameters.AddWithValue("@id", expense.Id.ToString());
            command.Parameters.AddWithValue("@amount", expense.Amount.ToString());
            command.Parameters.AddWithValue("@description", expense.Description.ToString());
            command.Parameters.AddWithValue("@categoryId", expense.CategoryId.ToString());
            command.Parameters.AddWithValue("@frequencyId", frequencyId);

            command.ExecuteNonQuery();
        }

        public void DeleteExpenses(List<string> expenseIds)
        {
            _logger.LogInformation("DeleteExpenses called for ExpenseIds: {ExpenseIds}", string.Join(", ", expenseIds));
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();

            var idPlaceholders = string.Join(", ", expenseIds.Select((_, i) => $"@id{i}"));
            command.CommandText = $"DELETE FROM Expenses WHERE id IN ({idPlaceholders})";

            for (int i = 0; i < expenseIds.Count; i++)
            {
                command.Parameters.AddWithValue($"@id{i}", expenseIds[i]);
            }

            command.ExecuteNonQuery();
        }

        public string GetCategoryNameById(string categoryId)
        {
            _logger.LogInformation("GetCategoryNameById called for CategoryId: {CategoryId}", categoryId);
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT name 
                FROM Categories 
                WHERE id = @categoryId";
            command.Parameters.AddWithValue("@categoryId", categoryId);

            return command.ExecuteScalar()?.ToString();
        }

        public List<ExpenseWithCategoryDto> GetExpensesByUserId(string userId)
        {
            _logger.LogInformation("GetExpensesByUserId called for UserId: {UserId}", userId);
            var expenses = new List<ExpenseWithCategoryDto>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT 
                        Expenses.id, 
                        Expenses.amount, 
                        Expenses.description, 
                        Expenses.categoryId, 
                        Categories.name AS categoryName, 
                        Expenses.frequencyId, 
                        Expenses.createdDate
                    FROM Expenses
                    INNER JOIN Categories ON Expenses.categoryId = Categories.id
                    WHERE Expenses.userId = @userId";
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(new ExpenseWithCategoryDto
                        {
                            Id = reader["id"]?.ToString(),
                            Amount = Convert.ToDecimal(reader["amount"]),
                            Description = reader["description"].ToString(),
                            CategoryId = reader["categoryId"].ToString(),
                            CategoryName = reader["categoryName"].ToString(),
                            Frequency = (Frequency)(int)(long)reader["frequencyId"],
                            Date = DateTime.Parse(reader["createdDate"].ToString())
                        });
                    }
                }
            }

            return expenses;
        }

        public List<Models.Category> GetCategoriesByUserId(string userId)
        {
            _logger.LogInformation("GetCategoriesByUserId called for UserId: {UserId}", userId);
            var categories = new List<Models.Category>();

            var defaultCategories = GetDefaultCategories();
            categories.AddRange(defaultCategories);

            var userCategories = GetUserCategories(userId);
            categories.AddRange(userCategories);

            return categories;
        }

        private Models.Category[] GetDefaultCategories()
        {
            _logger.LogInformation("GetDefaultCategories called.");
            var defaultCategories = new List<Models.Category>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT id, name, isDefault, CreatedBy
                    FROM Categories
                    WHERE isDefault = 1";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        defaultCategories.Add(new Models.Category
                        {
                            Id = reader["id"]?.ToString(),
                            Name = reader["name"].ToString(),
                            IsDefault = Convert.ToBoolean(reader["isDefault"]),
                            CreatedBy = reader["createdBy"]?.ToString(),
                        });
                    }
                }
            }

            return defaultCategories.ToArray();
        }

        private Models.Category[] GetUserCategories(string userId)
        {
            _logger.LogInformation("GetUserCategories called for UserId: {UserId}", userId);
            var userCategories = new List<Models.Category>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT id, name, isDefault, CreatedBy
                    FROM Categories
                    WHERE createdBy = @userId";
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userCategories.Add(new Models.Category
                        {
                            Id = reader["id"]?.ToString(),
                            Name = reader["name"].ToString(),
                            IsDefault = Convert.ToBoolean(reader["isDefault"]),
                            CreatedBy = reader["createdBy"]?.ToString(),
                        });
                    }
                }
            }

            return userCategories.ToArray(); 
        }

        public void AddCategory(Models.Category category)
        {
            _logger.LogInformation("AddCategory called for CategoryId: {CategoryId}, CreatedBy: {CreatedBy}", category.Id, category.CreatedBy);
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Enabling foreign key enforcement for SQLite, which is disabled by default
                var command = connection.CreateCommand();
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                CheckUser(category.CreatedBy.ToString());
        
                command.CommandText = @"
                    INSERT INTO Categories (id, name, isDefault, createdBy)
                    VALUES (@id, @name, @isDefault, @createdBy)";
                
                command.Parameters.AddWithValue("@id", category.Id);
                command.Parameters.AddWithValue("@name", category.Name);
                command.Parameters.AddWithValue("@isDefault", category.IsDefault);
                command.Parameters.AddWithValue("@createdBy", category.CreatedBy.ToString());

                command.ExecuteNonQuery();
            }
        }

        private void CheckUser(string userId)
        {
            _logger.LogInformation("CheckUser called for UserId: {UserId}", userId);
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                var checkUserCommand = connection.CreateCommand();
                checkUserCommand.CommandText = "SELECT COUNT(1) FROM Users WHERE id = @userId";
                checkUserCommand.Parameters.AddWithValue("@userId", userId);
                
                var userExists = (long)checkUserCommand.ExecuteScalar() > 0;
                if (!userExists)
                {
                    throw new Exception($"User with ID {userId} does not exist.");
                }
            }
        }
    }
}