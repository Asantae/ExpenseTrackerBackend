using ExpenseTrackerBackend.Enums;

namespace ExpenseTrackerBackend.Dtos;

public class ExpenseWithCategoryDto
{
    public string Id { get; set; }
    public string CreatedBy { get; set; }
    public string CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public Frequency Frequency { get; set; }
    public DateTime? ExpenseDate { get; set; }
    public string CategoryName { get; set; }
}