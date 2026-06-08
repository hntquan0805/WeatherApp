using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.API.DTOs;
using WeatherApp.API.Services;
using WeatherApp.API.Services.Interfaces;

namespace WeatherApp.API.Controllers;

public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService    authService,
        JwtTokenService jwtService) : base(jwtService)
    {
        _authService = authService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponseDto<object>.Fail(
                "Dữ liệu không hợp lệ.",
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));

        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Success(result, "Đăng ký thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return Fail(ex.Message);
        }
    }

    // POST api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponseDto<object>.Fail(
                "Dữ liệu không hợp lệ.",
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()));

        try
        {
            var result = await _authService.LoginAsync(dto);
            return Success(result, "Đăng nhập thành công.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fail(ex.Message, 401);
        }
    }

    // GET api/auth/profile
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _authService.GetProfileAsync(userId);
            return Success(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Fail(ex.Message, 404);
        }
    }

    // PUT api/auth/profile
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<UserDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _authService.UpdateProfileAsync(userId, dto);
            return Success(result, "Cập nhật profile thành công.");
        }
        catch (InvalidOperationException ex)
        {
            return Fail(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fail(ex.Message, 401);
        }
        catch (KeyNotFoundException ex)
        {
            return Fail(ex.Message, 404);
        }
    }
}