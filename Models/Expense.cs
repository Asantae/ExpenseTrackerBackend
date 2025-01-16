using ExpenseTrackerBackend.Enums;

namespace ExpenseTrackerBackend.Models;
public class Expense
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Category CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public Frequency Frequency { get; set; }
}