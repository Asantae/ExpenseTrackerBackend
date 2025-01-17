using System.IdentityModel.Tokens.Jwt;
using ExpenseTrackerBackend.Models;
using ExpenseTrackerBackend.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTrackerBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtTokenUtility _jwtTokenUtility;

    public AuthController(JwtTokenUtility jwtTokenUtility)
    {
        _jwtTokenUtility = jwtTokenUtility;
    }
    
    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest("Token and Refresh Token must be provided.");
        }

        var isValidRefreshToken = _jwtTokenUtility.ValidateRefreshToken(request.RefreshToken);
        if (!isValidRefreshToken)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        var jwtHandler = new JwtSecurityTokenHandler();
        var jwtToken = jwtHandler.ReadJwtToken(request.Token);

        if (jwtToken == null)
        {
            return Unauthorized("Invalid token.");
        }

        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Invalid token claims.");
        }

        var newJwtToken = _jwtTokenUtility.GenerateToken(userId);
        var newRefreshToken = _jwtTokenUtility.GenerateNewRefreshToken(userId);

        return Ok(new
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken
        });
    }
}