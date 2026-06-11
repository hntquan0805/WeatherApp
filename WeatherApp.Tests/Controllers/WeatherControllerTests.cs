using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WeatherApp.API.Controllers;
using WeatherApp.API.DTOs;
using WeatherApp.API.Services;
using WeatherApp.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace WeatherApp.Tests.Controllers;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _weatherServiceMock = new();
    private readonly WeatherController     _controller;

    public WeatherControllerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"]       = "super-secret-key-for-testing-32chars!",
                ["JwtSettings:Issuer"]          = "TestIssuer",
                ["JwtSettings:Audience"]        = "TestAudience",
                ["JwtSettings:ExpiryInMinutes"] = "60",
            })
            .Build();

        var jwtService = new JwtTokenService(config);
        _controller    = new WeatherController(_weatherServiceMock.Object, jwtService);

        // Giả lập user đã đăng nhập với userId = 1
        SetAuthenticatedUser(userId: 1);
    }

    private void SetAuthenticatedUser(int userId)
    {
        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "User"),
        };
        var identity  = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private static WeatherDto FakeWeather(string city = "Hanoi") => new()
    {
        City        = city,
        Country     = "VN",
        Temperature = 30,
        FeelsLike   = 33,
        TempMin     = 28,
        TempMax     = 32,
        Humidity    = 75,
        WindSpeed   = 3.5,
        Pressure    = 1010,
        Visibility  = 10000,
        Description = "clear sky",
        Icon        = "01d",
        Unit        = "°C",
        Sunrise     = DateTime.UtcNow,
        Sunset      = DateTime.UtcNow.AddHours(12),
    };

    // ── GetWeather ────────────────────────────────────
    [Fact]
    public async Task GetWeather_ValidCity_Returns200()
    {
        _weatherServiceMock
            .Setup(s => s.SaveAndGetWeatherAsync("Hanoi", 1, "metric"))
            .ReturnsAsync(FakeWeather("Hanoi"));

        var result = await _controller.GetWeather("Hanoi", "metric") as OkObjectResult;

        result!.StatusCode.Should().Be(200);
        var body = result.Value as ApiResponseDto<WeatherDto>;
        body!.Data!.City.Should().Be("Hanoi");
    }

    [Fact]
    public async Task GetWeather_EmptyCity_Returns400()
    {
        var result = await _controller.GetWeather("", "metric") as ObjectResult;

        result!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetWeather_CityNotFound_Returns404()
    {
        _weatherServiceMock
            .Setup(s => s.SaveAndGetWeatherAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("Không tìm thấy thành phố."));

        var result = await _controller.GetWeather("XYZCity", "metric") as ObjectResult;

        result!.StatusCode.Should().Be(404);
    }

    // ── GetHistory ────────────────────────────────────
    [Fact]
    public async Task GetHistory_AuthenticatedUser_Returns200WithLogs()
    {
        var logs = new List<WeatherLogDto>
        {
            new() { Id=1, City="Hanoi",  Country="VN", Temperature=30,
                    Humidity=70, Description="sunny", Icon="01d" },
            new() { Id=2, City="Danang", Country="VN", Temperature=28,
                    Humidity=65, Description="cloudy", Icon="02d" },
        };

        _weatherServiceMock.Setup(s => s.GetHistoryAsync(1))
                           .ReturnsAsync(logs);

        var result = await _controller.GetHistory() as OkObjectResult;

        result!.StatusCode.Should().Be(200);
        var body = result.Value as ApiResponseDto<IEnumerable<WeatherLogDto>>;
        body!.Data.Should().HaveCount(2);
    }

    // ── DeleteHistory ─────────────────────────────────
    [Fact]
    public async Task DeleteHistory_AuthenticatedUser_Returns200()
    {
        _weatherServiceMock.Setup(s => s.DeleteHistoryAsync(1))
                           .Returns(Task.CompletedTask);

        var result = await _controller.DeleteHistory() as OkObjectResult;

        result!.StatusCode.Should().Be(200);
        _weatherServiceMock.Verify(s => s.DeleteHistoryAsync(1), Times.Once);
    }
}