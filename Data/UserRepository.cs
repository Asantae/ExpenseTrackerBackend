using System;
using System.Collections.Generic;
using System.Data.SQLite;
using ExpenseTrackerBackend.Models;

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
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Users (id, username, password, email) VALUES (@Id, @Username, @Password, @Email)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id.ToString());
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Password", user.Password);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.ExecuteNonQuery();
                }
            }
        }

        public User GetUserByUsername(string username)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT id, username, password, email FROM Users WHERE username = @Username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
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
    }
}
