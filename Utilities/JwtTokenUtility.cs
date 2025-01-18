using Microsoft.IdentityModel.Tokens;
using System.Data.SQLite;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTrackerBackend.Utilities;

public class JwtTokenUtility
{
    private readonly string _secretKey;
    private readonly string _refreshTokenSecretKey;
    private readonly string _issuer;
    private readonly string _connectionString;

    public JwtTokenUtility(string secretKey, string refreshTokenSecretKey, string issuer, string connectionString)
    {
        _secretKey = secretKey;
        _refreshTokenSecretKey = refreshTokenSecretKey;
        _issuer = issuer;
        _connectionString = connectionString;
    }

    public string GenerateToken(string userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Iat, 
                  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                  ClaimValueTypes.Integer64)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _issuer,
            audience: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*)
                FROM RefreshTokens
                WHERE token = @token AND expiresAt > @currentDate AND isRevoked = 0";
            command.Parameters.AddWithValue("@token", refreshToken);
            command.Parameters.AddWithValue("@currentDate", DateTime.UtcNow);

            var count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }

    public string GenerateNewRefreshToken(string userId)
    {
        var refreshTokenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_refreshTokenSecretKey));
        var credentials = new SigningCredentials(refreshTokenKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddDays(1);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Iat, 
                  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                  ClaimValueTypes.Integer64)
        };

        var refreshTokenDescription = new JwtSecurityToken(
            issuer: _issuer,
            audience: _issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials
        );

        var refreshToken = new JwtSecurityTokenHandler().WriteToken(refreshTokenDescription);

        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO RefreshTokens (id, userId, token, expiresAt, isRevoked)
                VALUES (@id, @userId, @token, @expiresAt, 0)";
            command.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@token", refreshToken);
            command.Parameters.AddWithValue("@expiresAt", expiresAt);

            command.ExecuteNonQuery();
        }
        
        return refreshToken;
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE RefreshTokens
                SET isRevoked = 1
                WHERE token = @token";
            command.Parameters.AddWithValue("@token", refreshToken);

            command.ExecuteNonQuery();
        }
    }

    public string GetUserIdFromToken(string token)
    {
         if (string.IsNullOrEmpty(token))
    {
        throw new ArgumentException("Token cannot be null or empty", nameof(token));
    }

    try
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var jwtToken = tokenHandler.ReadJwtToken(token);

        var userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("User ID claim not found in token");
        }

        return userId;
    }
    catch (Exception ex)
    {
        throw new Exception("Failed to extract user ID from token", ex);
    }
    }
}