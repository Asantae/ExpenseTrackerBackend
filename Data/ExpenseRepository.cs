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

    }
}