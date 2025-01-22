namespace ExpenseTrackerBackend.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsDefault { get; set; }
    public Guid CreatedBy { get; set; }
}