using Demo1.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Demo1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generate JWT using API Key authentication.
    /// </summary>
    [Authorize(AuthenticationSchemes = "ApiKey")]
    [HttpPost("token")]
    public IActionResult GenerateToken(
        TokenRequest request)
    {
        var claims = new List<Claim>
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                request.UserId),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new Claim(
                ClaimTypes.NameIdentifier,
                request.UserId),

            new Claim(
                ClaimTypes.Name,
                request.Name),

            new Claim(
                ClaimTypes.Role,
                request.Role),

            new Claim(
                "client_id",
                request.ClientId),

            new Claim(
                "scope",
                "profile.read")
        };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var expiryMinutes =
            int.Parse(
                _configuration[
                    "Jwt:ExpiryMinutes"]!);

        var now =
            DateTime.UtcNow;

        var token =
            new JwtSecurityToken(
                issuer:
                    _configuration["Jwt:Issuer"],

                audience:
                    _configuration["Jwt:Audience"],

                claims:
                    claims,

                notBefore:
                    now,

                expires:
                    now.AddMinutes(
                        expiryMinutes),

                signingCredentials:
                    credentials);

        var jwt =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return Ok(new
        {
            access_token = jwt,
            token_type = "Bearer",
            expires_in =
                expiryMinutes * 60
        });
    }
}
