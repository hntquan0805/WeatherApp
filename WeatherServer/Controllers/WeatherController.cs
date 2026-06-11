using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherApp.API.DTOs;
using WeatherApp.API.Services;
using WeatherApp.API.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

namespace WeatherApp.API.Controllers;

[Authorize]
[EnableRateLimiting("weather")]
public class WeatherController : BaseController
{
    private readonly IWeatherService _weatherService;

    public WeatherController(
        IWeatherService weatherService,
        JwtTokenService jwtService) : base(jwtService)
    {
        _weatherService = weatherService;
    }

    // GET api/weather?city=Hanoi&units=metric
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponseDto<WeatherDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
    public async Task<IActionResult> GetWeather(
        [FromQuery] string city,
        [FromQuery] string units = "metric")
    {
        if (string.IsNullOrWhiteSpace(city))
            return Fail("Tên thành phố không được để trống.");

        try
        {
            // Nếu đã đăng nhập thì lưu lịch sử, chưa đăng nhập chỉ xem
            var userIdClaim = _jwtService.GetUserIdFromToken(User);

            WeatherDto result;
            if (userIdClaim.HasValue)
                result = await _weatherService
                    .SaveAndGetWeatherAsync(city, userIdClaim.Value, units);
            else
                result = await _weatherService.GetWeatherAsync(city, units);

            return Success(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Fail(ex.Message, 404);
        }
    }

    // POST api/weather/search
    [HttpPost("search")]
    [ProducesResponseType(typeof(ApiResponseDto<WeatherDto>), 200)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 404)]
    public async Task<IActionResult> Search([FromBody] WeatherRequestDto dto)
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
            var userId = GetCurrentUserId();
            var result = await _weatherService
                .SaveAndGetWeatherAsync(dto.City, userId, dto.Units);
            return Success(result);
        }
        catch (KeyNotFoundException ex)
        {
            return Fail(ex.Message, 404);
        }
    }

    // GET api/weather/history
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<WeatherLogDto>>), 200)]
    public async Task<IActionResult> GetHistory()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _weatherService.GetHistoryAsync(userId);
            return Success(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fail(ex.Message, 401);
        }
    }

    // GET api/weather/history/recent?count=5
    [HttpGet("history/recent")]
    [ProducesResponseType(typeof(ApiResponseDto<IEnumerable<WeatherLogDto>>), 200)]
    public async Task<IActionResult> GetRecentHistory([FromQuery] int count = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _weatherService.GetRecentHistoryAsync(userId, count);
            return Success(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fail(ex.Message, 401);
        }
    }

    // DELETE api/weather/history
    [HttpDelete("history")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), 200)]
    public async Task<IActionResult> DeleteHistory()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _weatherService.DeleteHistoryAsync(userId);
            return Success<object>(null!, "Đã xoá toàn bộ lịch sử.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fail(ex.Message, 401);
        }
    }
}