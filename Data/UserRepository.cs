using System.Data.SQLite;
using Dapper;
using ExpenseTrackerBackend.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackerBackend.Repositories;
public class UserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(string connectionString, ILogger<UserRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

public void AddUser(User user)
{
    _logger.LogInformation("AddUser called for UserId: {UserId}, Username: {Username}", user.Id, user.Username);
    
    try
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (id, username, password, email, created_at)
            VALUES (@id, @username, @password, @email, @createdAt)";
        
        command.Parameters.AddWithValue("@id", user.Id.ToString());
        command.Parameters.AddWithValue("@username", user.Username);
        command.Parameters.AddWithValue("@password", user.Password);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@createdAt", user.CreatedAt);

        command.ExecuteNonQuery();

        _logger.LogInformation("User added successfully for UserId: {UserId}, Username: {Username}", user.Id, user.Username);
    }
    catch (SqliteException ex)
    {
        _logger.LogError(ex, "SQLite error occurred while adding user with UserId: {UserId}, Username: {Username}", user.Id, user.Username);
        throw; // rethrow the exception to propagate it if necessary
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while adding user with UserId: {UserId}, Username: {Username}", user.Id, user.Username);
        throw; // rethrow the exception to propagate it if necessary
    }
}

    public void AddGuest(User user)
    {
        _logger.LogInformation("AddGuest called for UserId: {UserId}, Username: {Username}", user.Id, user.Username);
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Users
            SET username = @username,
                password = @password,
                email = @email,
                created_at = @createdAt
            WHERE id = @id";

        command.Parameters.AddWithValue("@id", user.Id.ToString());
        command.Parameters.AddWithValue("@username", user.Username);
        command.Parameters.AddWithValue("@password", user.Password);
        command.Parameters.AddWithValue("@email", user.Email);
        command.Parameters.AddWithValue("@createdAt", user.CreatedAt);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected == 0)
        {
            throw new Exception($"User with ID {user.Id} not found.");
        }
    }

    public User GetUserByUsername(string username)
    {
        _logger.LogInformation("GetUserByUsername called for Username: {Username}", username);
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT id, username, password, email, created_at
            FROM Users
            WHERE username = @username";
        command.Parameters.AddWithValue("@username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = Guid.Parse(reader.GetString(0)),
                Username = reader.GetString(1),
                Password = reader.GetString(2),
                Email = reader.GetString(3),
                CreatedAt = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public User GetUserById(string id)
    {
        _logger.LogInformation("GetUserById called for Id: {Id}", id);
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT id, username, password, email FROM Users WHERE id = @Id";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = Guid.Parse(reader.GetString(0)),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2),
                            Email = reader.GetString(3)
                        };
                    }
                }
            }
        }
        return null;
    }

    public bool IsUsernameTaken(string username)
    {
        _logger.LogInformation("IsUsernameTaken called for Username: {Username}", username);
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT COUNT(*) FROM Users WHERE username = @Username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }
    }

    public bool IsEmailTaken(string email)
    {
        _logger.LogInformation("IsEmailTaken called for Email: {Email}", email);
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT COUNT(1) FROM Users WHERE email = @Email";
            var result = connection.ExecuteScalar<int>(query, new { Email = email });
            return result > 0;
        }
    }
}