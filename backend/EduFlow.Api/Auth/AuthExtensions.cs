using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace EduFlow.Api.Auth;

public static class AuthExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static IActionResult Forbidden(string msg = "Forbidden") => new ObjectResult(msg) { StatusCode = 403 };
}