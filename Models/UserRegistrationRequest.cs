namespace ExpenseTrackerBackend.Models;
public class UserRegistrationRequest
{
    public string? UserId { get; set; } 
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}
