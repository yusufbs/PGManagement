using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PGManagement.API.Services;
using PGManagement.Contracts.Requests.Auth;
using PGManagement.Contracts.Responses.Auth;
using PGManagement.Contracts.Responses.Common;
using PGManagement.Infrastructure.Persistence;

namespace PGManagement.API.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest(ApiResponse<AuthResponse>.Fail("User with this email already exists."));

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return BadRequest(ApiResponse<AuthResponse>.Fail(errors));
        }

        var authResponse = _tokenService.GenerateToken(user);
        return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "User registered successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid email or password."));

        var authResponse = _tokenService.GenerateToken(user);
        return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "Login successful."));
    }
}
