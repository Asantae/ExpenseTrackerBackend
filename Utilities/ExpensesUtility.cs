using System.Data.SQLite;
using ExpenseTrackerBackend.Enums;
using ExpenseTrackerBackend.Models;

namespace ExpenseTrackerBackend.Utilities;

public class ExpensesUtility
{
    private readonly string _connectionString;

    public ExpensesUtility(string connectionString)
    {
        _connectionString = connectionString;
    }
}