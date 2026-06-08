using Microsoft.AspNetCore.Mvc;
using WeatherApp.API.DTOs;
using WeatherApp.API.Services;

namespace WeatherApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly JwtTokenService _jwtService;

    protected BaseController(JwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    // Lấy userId từ JWT token đang đăng nhập
    protected int GetCurrentUserId()
    {
        var userId = _jwtService.GetUserIdFromToken(User);
        if (userId is null)
            throw new UnauthorizedAccessException("Không xác định được user.");
        return userId.Value;
    }

    // Wrapper response thống nhất
    protected IActionResult Success<T>(T data, string message = "Thành công") =>
        Ok(ApiResponseDto<T>.Ok(data, message));

    protected IActionResult Fail(string message, int statusCode = 400)
    {
        var response = ApiResponseDto<object>.Fail(message);
        return StatusCode(statusCode, response);
    }
}