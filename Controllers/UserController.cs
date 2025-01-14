using Microsoft.AspNetCore.Mvc;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Repositories;
using ExpenseTrackerBackend.Utilities;
using DotNetEnv;
using System;

namespace ExpenseTrackerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly UserRepository _userRepository;

        public UserController(IConfiguration config)
        {
            Env.Load();
            string connectionString = Env.GetString("CONNECTION_STRING");

            string secretKey = config.GetSection("Appsettings:SecretKey").Value;
            string issuer = config.GetSection("Appsettings:Issuer").Value;
            _jwtTokenGenerator = new JwtTokenGenerator(secretKey, issuer);

            _userRepository = new UserRepository(connectionString);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and Password are required.");
            }

            if (_userRepository.IsUsernameTaken(request.Username))
            {
                return Conflict("User already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                CreatedAt = DateTime.UtcNow
            };

            _userRepository.AddUser(user);

            return Ok(new { Message = "User registered successfully.", UserId = user.Id });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            var user = _userRepository.GetUserByUsername(request.Username);
            if (user == null || user.Password != request.Password)
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.Username);

            return Ok(new { Message = "Login successful.", Token = token });
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

            var token = _jwtTokenGenerator.GenerateToken(guest.Id.ToString(), guest.Username);
            return Ok(new { Message = "Logged in as guest.", Token = token });
        }
    }
}
