using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;
using System.Collections.Generic;

namespace ExpenseTrackerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private string connectionString = "Data Source=C:\\Users\\Asantae\\ExpenseTrackerBackend\\expense_tracker.db;Version=3;";

        [HttpGet("testConnection")]
        public IActionResult TestConnection()
        {
            try
            {
                // Try to open a connection to the SQLite database
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    return Ok("Connection successful!");
                }
            }
            catch (SQLiteException ex)
            {
                // If there’s an error, return the error message
                return StatusCode(500, $"Error connecting to database: {ex.Message}");
            }
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, name FROM categories";
                var command = new SQLiteCommand(query, connection);
                var reader = command.ExecuteReader();
                
                var categories = new List<object>();
                while (reader.Read())
                {
                    categories.Add(new
                    {
                        Id = reader["id"],
                        Name = reader["name"]
                    });
                }

                connection.Close();
                return Ok(categories);
            }
        }

        [HttpGet("expense")]
        public IActionResult GetExpense()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, amount, description, date FROM expense";
                var command = new SQLiteCommand(query, connection);
                var reader = command.ExecuteReader();
                
                var expense = new List<object>();
                while (reader.Read())
                {
                    expense.Add(new
                    {
                        Id = reader["id"],
                        Amount = reader["amount"],
                        Description = reader["description"],
                        Date = reader["date"]
                    });
                }

                connection.Close();
                return Ok(expense);
            }
        }
    }
}
