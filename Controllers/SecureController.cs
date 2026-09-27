using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo1.Controllers;
[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    /// <summary>
    /// Validate JWT and read claims.
    /// </summary>
    [Authorize(
        AuthenticationSchemes =
            JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var name =
            User.FindFirstValue(
                ClaimTypes.Name);

        var role =
            User.FindFirstValue(
                ClaimTypes.Role);

        var clientId =
            User.FindFirstValue(
                "client_id");

        var scope =
            User.FindFirstValue(
                "scope");

        return Ok(new
        {
            userId,
            name,
            role,
            clientId,
            scope
        });
    }


    /// <summary>
    /// Authorization using scope claim.
    /// </summary>
    [Authorize(
        Policy = "ProfileRead")]
    [HttpGet("profile-policy")]
    public IActionResult ProfilePolicy()
    {
        return Ok(new
        {
            message =
                "User mempunyai scope profile.read",

            user =
                User.Identity?.Name
        });
    }


    /// <summary>
    /// Authorization using role.
    /// </summary>
    [Authorize(
        Policy = "AdminOnly")]
    [HttpGet("admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            message =
                "Admin access granted."
        });
    }
}
