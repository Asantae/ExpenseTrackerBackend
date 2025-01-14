using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using ExpenseTrackerBackend.Models;
using Microsoft.Data.Sqlite;

namespace ExpenseTrackerBackend.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddUser(User user)
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
        }

        public User GetUserByUsername(string username)
        {
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
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT COUNT(1) FROM Users WHERE email = @Email";
                var result = connection.ExecuteScalar<int>(query, new { Email = email });
                return result > 0;
            }
        }

    }
}
