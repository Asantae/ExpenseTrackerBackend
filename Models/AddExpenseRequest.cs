using ExpenseTrackerBackend.Enums;

public class AddExpenseRequest
{
    public decimal Amount { get; set; }
    public string CategoryId { get; set; }
    public string Description { get; set; } = "";
    public Frequency Frequency { get; set; }
    public string? Date { get; set; }
}
