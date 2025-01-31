using ExpenseTrackerBackend.Dtos;
using ExpenseTrackerBackend.Enums;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Repositories;
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

    public ExpenseController(ExpenseRepository expenseRepository, IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
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
        if(!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var expenses = _expenseRepository.GetExpensesByUserId(userId);

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

        return Ok(new { Message = "Successfully added a new category", Category = newCategory });
    }

    [HttpPost("addExpense")]
    public IActionResult AddExpense([FromBody] Expense expense, [FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("Couldn't get userId");
        }

        if(!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var newExpense = new Expense
        {
            Id = Guid.NewGuid().ToString(),
            Amount = expense.Amount,
            Description = expense.Description,
            CategoryId = expense.CategoryId,
            Frequency = expense.Frequency,
            Date = DateTime.UtcNow,
            CreatedBy = userId,
        };

        _expenseRepository.AddExpense(newExpense);

        var categoryName = _expenseRepository.GetCategoryNameById(newExpense.CategoryId);
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return NotFound(new { Message = $"Category not found with CategoryId: {newExpense.CategoryId}." });
        }

        var expenseWithCategoryName = new ExpenseWithCategoryDto
        {
            Id = newExpense.Id,
            Amount = newExpense.Amount,
            Description = newExpense.Description,
            CategoryId = newExpense.CategoryId,
            CategoryName = categoryName,
            Frequency = newExpense.Frequency,
            Date = newExpense.Date
        };

        return Ok(new { Message = "Successfully added a new expense", Expense = expenseWithCategoryName });
    }
}