using Microsoft.AspNetCore.Mvc;
using ExpenseTrackerBackend.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTrackerBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private string connectionString;
        private readonly IConfiguration _config;

        public UserController(IConfiguration config)
        {
            Env.Load();
            connectionString = Env.GetString("CONNECTION_STRING");
            _config=config;
        }
        
        private static List<User> mockUsers = new List<User>();

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegistrationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and Password are required.");
            }

            var existingUser = mockUsers.FirstOrDefault(u => u.Username == request.Username);
            if (existingUser != null)
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

            mockUsers.Add(user);

            return Ok(new { Message = "User registered successfully.", UserId = user.Id });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginRequest request)
        {
            var user = mockUsers.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = Guid.NewGuid().ToString();

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

            mockUsers.Add(guest);

            var token = Guid.NewGuid().ToString(); 
            return Ok(new { Message = "Logged in as guest.", Token = token });
        }
    }
}