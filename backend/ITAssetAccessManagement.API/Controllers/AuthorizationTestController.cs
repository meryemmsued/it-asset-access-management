using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetAccessManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthorizationTestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new
        {
            Message = "Bu endpoint herkese açık."
        });
    }

    [Authorize]
    [HttpGet("authenticated")]
    public IActionResult AuthenticatedEndpoint()
    {
        return Ok(new
        {
            Message = "JWT doğrulaması başarılı. Giriş yapmış kullanıcı erişti.",
            UserId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value,
            Email = User.FindFirst(
                System.Security.Claims.ClaimTypes.Email
            )?.Value,
            Name = User.Identity?.Name
        });

    }

    [Authorize]
    [HttpGet("claims")]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(claim => new
        {
            claim.Type,
            claim.Value
        });

        return Ok(claims);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            Message = "Bu endpoint yalnızca Admin rolüne açık."
        });
    }
}