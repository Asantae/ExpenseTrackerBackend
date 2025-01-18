using ExpenseTrackerBackend.Enums;
using ExpenseTrackerBackend.Models;
using Microsoft.Data.Sqlite;

namespace ExpenseTrackerBackend.Repositories
{
    public class ExpenseRepository
    {
        private readonly string _connectionString;

        public ExpenseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddExpense(Expense expense)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Expenses (id, amount, description, categoryId, label, created_at)
                VALUES (@id, @amount, @description, @categoryId, label, @createdAt)";
            command.Parameters.AddWithValue("@id", expense.Id.ToString());
            command.Parameters.AddWithValue("@amount", expense.Amount.ToString());
            command.Parameters.AddWithValue("@description", expense.Description.ToString());
            command.Parameters.AddWithValue("@categoryId", expense.CategoryId.ToString());
            command.Parameters.AddWithValue("@label", expense.Frequency.ToString());
            command.Parameters.AddWithValue("@createdAt", expense.Date);

            command.ExecuteNonQuery();
        }

        public List<Expense> GetExpensesByUserId(Guid userId)
        {
            var expenses = new List<Expense>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT id, amount, description, categoryId, frequencyId, date
                    FROM Expenses
                    WHERE userId = @userId";
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(new Expense
                        {
                            Id = Guid.TryParse(reader["id"]?.ToString(), out var id) 
                                ? id 
                                : Guid.Empty,
                            Amount = Convert.ToDecimal(reader["amount"]),
                            Description = reader["description"].ToString(),
                            CategoryId = Convert.ToInt32(reader["categoryId"]),
                            Frequency = (Frequency)reader["frequency"],
                            Date = DateTime.Parse(reader["date"].ToString())
                        });
                    }
                }
            }

            return expenses;
        }

        public List<Models.Category> GetCategoriesByUserId(Guid userId)
        {
            var categories = new List<Models.Category>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT id, name, isDefault, CreatedBy
                    FROM Categories
                    WHERE createdBy = @userId OR isDefault = 1";
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Models.Category
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            IsDefault = Convert.ToBoolean(reader["isDefault"]),
                            CreatedBy = Guid.TryParse(reader["createdBy"]?.ToString(), out var createdByGuid) 
                                ? createdByGuid 
                                : Guid.Empty,
                        });
                    }
                }
            }

            return categories;
        }
    }
}