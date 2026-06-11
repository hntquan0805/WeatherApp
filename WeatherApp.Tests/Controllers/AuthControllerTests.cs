using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherApp.API.Controllers;
using WeatherApp.API.DTOs;
using WeatherApp.API.Services;
using WeatherApp.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace WeatherApp.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthController     _controller;

    public AuthControllerTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"]      = "super-secret-key-for-testing-32chars!",
                ["JwtSettings:Issuer"]         = "TestIssuer",
                ["JwtSettings:Audience"]       = "TestAudience",
                ["JwtSettings:ExpiryInMinutes"]= "60",
            })
            .Build();

        var jwtService = new JwtTokenService(config);
        _controller    = new AuthController(_authServiceMock.Object, jwtService);
    }

    private static AuthResponseDto FakeAuthResponse() => new()
    {
        Token     = "fake.jwt.token",
        ExpiresAt = DateTime.UtcNow.AddHours(1),
        User      = new UserDto
        {
            Id       = 1,
            Username = "testuser",
            Email    = "test@example.com",
            Role     = "User",
        },
    };

    // ── Register ──────────────────────────────────────
    [Fact]
    public async Task Register_ValidDto_Returns200WithToken()
    {
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                        .ReturnsAsync(FakeAuthResponse());

        var dto = new RegisterDto
        {
            Username        = "testuser",
            Email           = "test@example.com",
            Password        = "Password1",
            ConfirmPassword = "Password1",
        };

        var result = await _controller.Register(dto) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);

        var body = result.Value as ApiResponseDto<AuthResponseDto>;
        body!.Success.Should().BeTrue();
        body.Data.Token.Should().Be("fake.jwt.token");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        _authServiceMock.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                        .ThrowsAsync(new InvalidOperationException("Email đã được sử dụng."));

        var dto = new RegisterDto
        {
            Username        = "user",
            Email           = "dup@example.com",
            Password        = "Password1",
            ConfirmPassword = "Password1",
        };

        var result = await _controller.Register(dto) as ObjectResult;

        result!.StatusCode.Should().Be(400);
        var body = result.Value as ApiResponseDto<object>;
        body!.Success.Should().BeFalse();
        body.Message.Should().Contain("Email");
    }

    // ── Login ─────────────────────────────────────────
    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                        .ReturnsAsync(FakeAuthResponse());

        var result = await _controller.Login(new LoginDto
        {
            Email    = "test@example.com",
            Password = "Password1",
        }) as OkObjectResult;

        result!.StatusCode.Should().Be(200);
        var body = result.Value as ApiResponseDto<AuthResponseDto>;
        body!.Data!.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Login_WrongCredentials_Returns401()
    {
        _authServiceMock.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                        .ThrowsAsync(new UnauthorizedAccessException("Sai thông tin."));

        var result = await _controller.Login(new LoginDto
        {
            Email    = "wrong@example.com",
            Password = "WrongPass1",
        }) as ObjectResult;

        result!.StatusCode.Should().Be(401);
    }
}