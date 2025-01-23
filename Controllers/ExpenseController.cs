using ExpenseTrackerBackend.Enums;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Repositories;
using ExpenseTrackerBackend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTrackerBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ExpenseRepository _expenseRepository;

    public ExpenseController(ExpenseRepository expenseRepository, IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _expenseRepository = expenseRepository;
    }
    
    // [HttpGet("testConnection")]
    // public IActionResult TestConnection()
    // {
    //     try
    //     {
    //         using (var connection = new SQLiteConnection(connectionString))
    //         {
    //             connection.Open();
    //             return Ok("Connection successful!");
    //         }
    //     }
    //     catch (SQLiteException ex)
    //     {
    //         return StatusCode(500, $"Error connecting to database: {ex.Message}");
    //     }
    // }

    // [HttpGet("testAuth")]
    // public IActionResult TestAuth()
    // {
    //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //     if (userId == null)
    //     {
    //         return Unauthorized("Invalid token.");
    //     }

    //     return Ok(new { Message = "Token is valid!", UserId = userId });
    // }

    [HttpGet("getFrequencies")]
    public IActionResult GetFrequencies()
    {
        var frequencies = Enum.GetValues(typeof(Frequency))
            .Cast<Frequency>()
            .Select(f => new
            {
                Id = (int)f,
                Value = f.ToString()
            });

        return Ok(new { Message = "Successfully retrieved frequencies", Frequencies = frequencies });
    }

    [HttpGet("getCategories")]
    public IActionResult GetCategories([FromQuery] string userId)
    {
        if (!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var categories = _expenseRepository.GetCategoriesByUserId(userId);

        return Ok(new { Message = "Successfully retrieved categories", Categories = categories });
    }

    [HttpGet("getExpenses")]
    public IActionResult GetExpense([FromQuery] string userId)
    {
        if(!Guid.TryParse(userId, out var parsedUserId))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var expenses = _expenseRepository.GetExpensesByUserId(parsedUserId);

        return Ok(new { Message = "Successfully retrieved expenses", Expenses = expenses });
    }

    [HttpPost("addCategory")]
    public IActionResult AddCategory([FromBody] Models.Category category, [FromQuery] string userId)
    {
        if(!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var newCategory = new Models.Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = category.Name,
            IsDefault = false,
            CreatedBy = userId,
        };

        _expenseRepository.AddCategory(newCategory);

        return Ok(new { Message = "Successfully added a new category", Category = category});
    }

    [HttpPost("addExpense")]
    public IActionResult AddExpense([FromBody] Expense expense)
    {
        if (string.IsNullOrWhiteSpace(expense.CreatedBy.ToString()))
        {
            return BadRequest("Couldn't get userId");
        }

        var newExpense = new Expense
        {
            Id = Guid.NewGuid(),
            Amount = expense.Amount,
            Description = expense.Description,
            CategoryId = expense.CategoryId,
            Frequency = expense.Frequency,
            Date = DateTime.UtcNow,
            CreatedBy = expense.CreatedBy,
        };

        _expenseRepository.AddExpense(newExpense);

        return Ok(new { Message = "Successfully added a new expense", Expense = newExpense });
    }
}