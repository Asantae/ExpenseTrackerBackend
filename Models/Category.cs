namespace ExpenseTrackerBackend.Models;

public class Category
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsDefault { get; set; }
    public string CreatedBy { get; set; } = "";
}