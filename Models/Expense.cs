using ExpenseTrackerBackend.Enums;

namespace ExpenseTrackerBackend.Models;
public class Expense
{
    public string Id { get; set; }
    public string CreatedBy { get; set; }
    public string CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public DateTime? ExpenseDate { get; set; }
    public Frequency Frequency { get; set; }
}