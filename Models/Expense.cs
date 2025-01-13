using System;

namespace ExpenseTrackerBackend.Models;
public class Expense
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public string Frequency { get; set; } = "";
}