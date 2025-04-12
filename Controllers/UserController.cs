using Microsoft.AspNetCore.Mvc;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Repositories;
using ExpenseTrackerBackend.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;

namespace ExpenseTrackerBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly string connectionString;
    private readonly string secretKey;
    private readonly string refreshTokenSecretKey;
    private readonly string issuer;
    private readonly JwtTokenUtility _jwtTokenUtility;
    private readonly UserUtility _userUtility;
    private readonly UserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(UserRepository userRepository, IConfiguration config, ILogger<UserController> logger)
    {
        connectionString = config.GetConnectionString("DefaultConnection");
        secretKey = config.GetSection("Jwt:SecretKey").Value;
        refreshTokenSecretKey = config.GetSection("Jwt:RefreshTokenSecretKey").Value;
        issuer = config.GetSection("Jwt:Issuer").Value;
        _jwtTokenUtility = new JwtTokenUtility(secretKey, refreshTokenSecretKey, issuer, connectionString);
        _userUtility = new UserUtility(connectionString);
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet("testConnection")]
    public IActionResult TestConnection()
    {
        try
        {
            _logger.LogInformation("Attempting SQLite connection to {DbPath}", connectionString);
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                _logger.LogInformation("SQLite database connection successful.");
                return Ok(new { message = "SQLite database connection successful.", dbPath = connectionString });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite connection failed.");
            return StatusCode(500, new { message = "SQLite database connection failed.", error = ex.Message });
        }
    }

    [HttpPost("test-write")]
    public IActionResult TestWritePermission()
    {
        try
        {
            var newGuid = Guid.NewGuid();
            var uniquePart = newGuid.ToString().Substring(0, 8);

            var hashedPassword = PasswordUtility.HashPassword("Guest");

            var guest = new User
            {
                Id = newGuid,
                Username = "Guest" + uniquePart,
                Email = $"guest_{uniquePart}@example.com",
                Password = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _userRepository.AddUser(guest);

            var token = _jwtTokenUtility.GenerateToken(guest.Id.ToString());
            var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(guest.Id.ToString());

            _logger.LogInformation("Guest user created with username: {Username}", guest.Username);

            return Ok(new { Message = "Logged in as guest", Token = token, RefreshToken = refreshToken, User = guest });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Test failed: {ex.Message}");
        }
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegistrationRequest request)
    {
        _logger.LogInformation("Attempting to register user with email: {Email}, username: {Username}", request.Email, request.Username);

        request.Email = request.Email.ToLower();
        request.Username = request.Username.ToLower();

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Registration failed for username: {Username} - Missing required fields", request.Username);
            return BadRequest("Username, Password, and Email are required");
        }

        if (_userRepository.IsUsernameTaken(request.Username))
        {
            _logger.LogWarning("Registration failed for username: {Username} - Username already taken", request.Username);
            return Conflict("Username is already taken");
        }

        if (_userRepository.IsEmailTaken(request.Email))
        {
            _logger.LogWarning("Registration failed for email: {Email} - Email already in use", request.Email);
            return Conflict("Email is already in use");
        }

        var hashedPassword = PasswordUtility.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.AddUser(user);

        var token = _jwtTokenUtility.GenerateToken(user.Id.ToString());
        var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(user.Id.ToString());

        _logger.LogInformation("User registered successfully with username: {Username}", user.Username);

        return Ok(new { Message = "User registered successfully", User = user, Token = token, RefreshToken = refreshToken });
    }

    [HttpPatch("registerGuest")]
    public IActionResult RegisterGuest([FromBody] UserRegistrationRequest request, [FromQuery] string userId)
    {
        _logger.LogInformation("Attempting to register guest with email: {Email}, username: {Username}", request.Email, request.Username);

        request.Email = request.Email.ToLower();
        request.Username = request.Username.ToLower();

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Guest registration failed for username: {Username} - Missing required fields", request.Username);
            return BadRequest("Username, Password, and Email are required");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Guest registration failed - Missing userId");
            return BadRequest("Guest user must have a valid user id");
        }

        if (_userRepository.IsUsernameTaken(request.Username))
        {
            _logger.LogWarning("Guest registration failed for username: {Username} - Username already taken", request.Username);
            return Conflict("Username is already taken");
        }

        if (_userRepository.IsEmailTaken(request.Email))
        {
            _logger.LogWarning("Guest registration failed for email: {Email} - Email already in use", request.Email);
            return Conflict("Email is already in use");
        }

        var hashedPassword = PasswordUtility.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.Parse(userId),
            Username = request.Username,
            Email = request.Email,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.AddGuest(user);

        var token = _jwtTokenUtility.GenerateToken(user.Id.ToString());
        var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(user.Id.ToString());

        _logger.LogInformation("Guest registered successfully with username: {Username}", user.Username);

        return Ok(new { Message = "User registered successfully", User = user, Token = token, RefreshToken = refreshToken });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginRequest request)
    {
        _logger.LogInformation("Attempting to log in with username: {Username}", request.Username);

        request.Username = request.Username.ToLower();

        var user = _userRepository.GetUserByUsername(request.Username);
        if (user == null || !PasswordUtility.VerifyPassword(request.Password, user.Password))
        {
            _logger.LogWarning("Login failed for username: {Username} - Invalid username or password", request.Username);
            return Unauthorized("Invalid username or password");
        }

        var token = _jwtTokenUtility.GenerateToken(user.Id.ToString());
        var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(user.Id.ToString());

        _logger.LogInformation("User logged in successfully with username: {Username}", user.Username);

        return Ok(new { Message = "Login successful", User = user, Token = token, RefreshToken = refreshToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] UserLogoutRequest request)
    {
        _logger.LogInformation("Attempting to log out with refresh token: {RefreshToken}", request.RefreshToken);

        var userId = _jwtTokenUtility.GetUserIdFromToken(request.RefreshToken);
        
        if (userId != null)
        {
            _jwtTokenUtility.RevokeRefreshToken(request.RefreshToken);
            _logger.LogInformation("User logged out successfully for refresh token: {RefreshToken}", request.RefreshToken);
            return Ok(new { message = "Logout successful" });
        }

        _logger.LogWarning("Logout failed for refresh token: {RefreshToken} - Invalid refresh token", request.RefreshToken);
        return BadRequest(new { error = "Invalid refresh token" });
    }

    [HttpPost("guest")]
    public IActionResult Guest()
    {
        var newGuid = Guid.NewGuid();
        var uniquePart = newGuid.ToString().Substring(0, 8);

        var hashedPassword = PasswordUtility.HashPassword("Guest");

        var guest = new User
        {
            Id = newGuid,
            Username = "Guest" + uniquePart,
            Email = $"guest_{uniquePart}@example.com",
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.AddUser(guest);

        var token = _jwtTokenUtility.GenerateToken(guest.Id.ToString());
        var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(guest.Id.ToString());

        _logger.LogInformation("Guest user created with username: {Username}", guest.Username);

        return Ok(new { Message = "Logged in as guest", Token = token, RefreshToken = refreshToken, User = guest });
    }

    [HttpGet("getUser")]
    public IActionResult GetUser([FromBody] Guid userId)
    {
        _logger.LogInformation("Retrieving user with id: {UserId}", userId);

        var user = _userUtility.GetUserById(userId);

        return Ok(new { Message = "Successfully retrieved user", User = user });
    }
}