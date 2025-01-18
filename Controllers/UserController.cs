using Microsoft.AspNetCore.Mvc;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Repositories;
using ExpenseTrackerBackend.Utilities;

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

    public UserController(IConfiguration config)
    {
        connectionString = config.GetConnectionString("DefaultConnection");
        secretKey = config.GetSection("Jwt:SecretKey").Value;
        refreshTokenSecretKey = config.GetSection("Jwt:RefreshTokenSecretKey").Value;
        issuer = config.GetSection("Jwt:Issuer").Value;
        _jwtTokenUtility = new JwtTokenUtility(secretKey, refreshTokenSecretKey, issuer, connectionString);
        _userUtility = new UserUtility(connectionString);
        _userRepository = new UserRepository(connectionString);
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Username, Password, and Email are required");
        }

        if (_userRepository.IsUsernameTaken(request.Username))
        {
            return Conflict("Username is already taken");
        }

        if (_userRepository.IsEmailTaken(request.Email))
        {
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

        var token = _jwtTokenUtility.GenerateToken(user.Id.ToString());
        var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(user.Id.ToString());

        _userRepository.AddUser(user);

        return Ok(new { Message = "User registered successfully", UserId = user.Id, Token = token, RefreshToken = refreshToken });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserLoginRequest request)
    {
        var user = _userRepository.GetUserByUsername(request.Username);
        if (user == null || !PasswordUtility.VerifyPassword(request.Password, user.Password))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _jwtTokenUtility.GenerateToken(user.Id.ToString());
        var refreshToken = _jwtTokenUtility.GenerateNewRefreshToken(user.Id.ToString());

        return Ok(new { Message = "Login successful", Token = token, RefreshToken = refreshToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] UserLogoutRequest request)
    {
        var userId = _jwtTokenUtility.GetUserIdFromToken(request.RefreshToken);
        
        if (userId != null)
        {
            _jwtTokenUtility.RevokeRefreshToken(request.RefreshToken);
            return Ok(new { message = "Logout successful" });
        }

        return BadRequest(new { error = "Invalid refresh token" });
    }

    [HttpGet("guest")]
    public IActionResult Guest()
    {
        var guest = new User
        {
            Id = Guid.NewGuid(),
            Username = "Guest",
            Email = "guest@example.com",
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.AddUser(guest);

        var token = _jwtTokenUtility.GenerateToken(guest.Id.ToString());
        return Ok(new { Message = "Logged in as guest", Token = token });
    }

    [HttpGet("getUser")]
    public IActionResult GetUser([FromBody] Guid userId)
    {
        var user = _userUtility.GetUserById(userId);

        return Ok(new { Message = "Successfully retrieved user", User = user });
    }
}