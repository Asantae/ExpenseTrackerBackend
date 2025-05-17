using ExpenseTrackerBackend.Dtos;
using ExpenseTrackerBackend.Enums;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExpenseTrackerBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExpenseController : ControllerBase
{
    private readonly string _connectionString;
    private readonly ExpenseRepository _expenseRepository;
    private readonly ILogger<ExpenseController> _logger;

    public ExpenseController(ExpenseRepository expenseRepository, IConfiguration config, ILogger<ExpenseController> logger)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
        _expenseRepository = expenseRepository;
        _logger = logger;
    }

    [HttpGet("getFrequencies")]
    public IActionResult GetFrequencies()
    {
        _logger.LogInformation("GetFrequencies request received.");
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
        _logger.LogInformation("GetCategories request received for userId: {UserId}", userId);
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
        _logger.LogInformation("GetExpenses request received for userId: {UserId}", userId);
        if (!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var expenses = _expenseRepository.GetExpensesByUserId(userId);

        return Ok(new { Message = "Successfully retrieved expenses", Expenses = expenses });
    }

    [HttpPost("addCategory")]
    public IActionResult AddCategory([FromBody] Models.Category category, [FromQuery] string userId)
    {
        _logger.LogInformation("AddCategory request received for userId: {UserId} with category name: {CategoryName}", userId, category.Name);
        if (!Guid.TryParse(userId, out var _))
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
    public IActionResult AddExpense([FromBody] AddExpenseRequest req, [FromQuery] string userId)
    {
        _logger.LogInformation("AddExpense request received for userId: {UserId} with expense amount: {Amount}", userId, req.Amount);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("Couldn't get userId");
        }

        if (!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }
        
        DateTime? dateValue = 
            !string.IsNullOrWhiteSpace(req.Date) && DateTime.TryParse(req.Date, out var dt)
                ? dt
                : (DateTime?)null;

        var newExpense = new Expense
        {
            Id = Guid.NewGuid().ToString(),
            Amount = req.Amount,
            Description = req.Description,
            CategoryId = req.CategoryId,
            Frequency = req.Frequency,
            ExpenseDate = dateValue,
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
            ExpenseDate = newExpense.ExpenseDate
        };

        return Ok(new { Message = "Successfully added a new expense", Expense = expenseWithCategoryName });
    }

    [HttpPatch("editExpense")]
    public IActionResult EditExpense([FromBody] Expense updatedExpense, [FromQuery] string userId)
    {
        _logger.LogInformation("EditExpense request received for userId: {UserId} with expense id: {ExpenseId}", userId, updatedExpense.Id);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("Couldn't get userId");
        }

        if (!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        if (!Guid.TryParse(updatedExpense.Id, out var _))
        {
            return BadRequest(new { Mesage = "Invalid expense id." });
        }

        _expenseRepository.UpdateExpense(updatedExpense);

        var categoryName = _expenseRepository.GetCategoryNameById(updatedExpense.CategoryId);
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return NotFound(new { Message = $"Category not found with CategoryId: {updatedExpense.CategoryId}." });
        }

        var expenseWithCategoryName = new ExpenseWithCategoryDto
        {
            Id = updatedExpense.Id,
            Amount = updatedExpense.Amount,
            Description = updatedExpense.Description,
            CategoryId = updatedExpense.CategoryId,
            CategoryName = categoryName,
            Frequency = updatedExpense.Frequency,
            ExpenseDate = updatedExpense.ExpenseDate
        };

        return Ok(new { Message = "Successfully added a new expense", Expense = expenseWithCategoryName });
    }

    [HttpDelete("deleteExpenses")]
    public IActionResult DeleteExpenses([FromQuery] string expenseIds, [FromQuery] string userId)
    {
        _logger.LogInformation("DeleteExpenses request received for userId: {UserId} with expenseIds: {ExpenseIds}", userId, expenseIds);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("Couldn't get userId");
        }

        if (!Guid.TryParse(userId, out var _))
        {
            return BadRequest(new { Message = "Invalid userId format." });
        }

        var expenseIdList = expenseIds.Split(',').ToList();
        _expenseRepository.DeleteExpenses(expenseIdList);

        return Ok(new { Message = $"Successfully deleted {expenseIdList.Count()} expense(s)", deletedExpenses = expenseIdList });
    }
}