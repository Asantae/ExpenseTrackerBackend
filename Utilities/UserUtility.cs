using System.Data.SQLite;
using ExpenseTrackerBackend.Models;

namespace ExpenseTrackerBackend.Utilities;

public class UserUtility
{
    private readonly string _connectionString;

    public UserUtility(string connectionString)
    {
        _connectionString = connectionString;
    }

    public User GetUserById(Guid userId)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, username, email
                FROM Users
                WHERE id = @id";
            command.Parameters.AddWithValue("@id", userId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return new User
                    {
                        Id = Guid.Parse(reader["id"].ToString()),
                        Username = reader["username"].ToString(),
                        Email = reader["email"].ToString()
                    };
                }
            }
        }
        return null;
    }
}