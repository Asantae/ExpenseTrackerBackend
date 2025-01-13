using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using System.Data.SQLite;

namespace ExpenseTrackerBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExpenseController : ControllerBase
{
    private string connectionString;

    public ExpenseController()
    {
        Env.Load();
        connectionString = Env.GetString("CONNECTION_STRING");
    }
    
    [HttpGet("testConnection")]
    public IActionResult TestConnection()
    {
        try
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                return Ok("Connection successful!");
            }
        }
        catch (SQLiteException ex)
        {
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
            string query = "SELECT id, amount, description, date FROM expenses";
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

    [HttpGet("checkTables")]
    public IActionResult CheckTables()
    {
    try
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT name FROM sqlite_master WHERE type='table'";
            var command = new SQLiteCommand(query, connection);
            var reader = command.ExecuteReader();

            var tables = new List<string>();
            while (reader.Read())
            {
                tables.Add(reader["name"].ToString());
            }

            connection.Close();
            return Ok(tables);
        }
    }
    catch (SQLiteException ex)
    {
        return StatusCode(500, $"Error checking tables: {ex.Message}");
    }
    }
}
