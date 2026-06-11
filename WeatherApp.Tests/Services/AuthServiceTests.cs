using FluentAssertions;
using Moq;
using WeatherApp.API.DTOs;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;
using WeatherApp.API.Services;
using WeatherApp.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace WeatherApp.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly AuthService           _service;
    private readonly JwtTokenService       _jwtService;

    public AuthServiceTests()
    {
        // Setup JwtTokenService với config giả
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"]       = "super-secret-key-for-testing-32chars!",
                ["JwtSettings:Issuer"]           = "TestIssuer",
                ["JwtSettings:Audience"]         = "TestAudience",
                ["JwtSettings:ExpiryInMinutes"]  = "60",
            })
            .Build();

        _jwtService = new JwtTokenService(config);
        _service    = new AuthService(
            _userRepoMock.Object,
            MapperHelper.Create(),
            _jwtService);
    }

    // ── Register ──────────────────────────────────────
    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.Id = 1; return u; });

        var dto = new RegisterDto
        {
            Username        = "newuser",
            Email           = "new@example.com",
            Password        = "Password1",
            ConfirmPassword = "Password1",
        };

        var result = await _service.RegisterAsync(dto);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("new@example.com");
        result.User.Username.Should().Be("newuser");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("dup@example.com"))
                     .ReturnsAsync(true);

        var dto = new RegisterDto
        {
            Username        = "user",
            Email           = "dup@example.com",
            Password        = "Password1",
            ConfirmPassword = "Password1",
        };

        var act = async () => await _service.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Email*");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsInvalidOperation()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UsernameExistsAsync("dupuser"))
                     .ReturnsAsync(true);

        var dto = new RegisterDto
        {
            Username        = "dupuser",
            Email           = "new@example.com",
            Password        = "Password1",
            ConfirmPassword = "Password1",
        };

        var act = async () => await _service.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Username*");
    }

    // ── Login ─────────────────────────────────────────
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Password1");
        var user = new User
        {
            Id           = 1,
            Username     = "testuser",
            Email        = "test@example.com",
            PasswordHash = hashedPassword,
            Role         = "User",
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
                     .ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginDto
        {
            Email    = "test@example.com",
            Password = "Password1",
        });

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Id.Should().Be(1);
    }

    [Fact]
    public async Task LoginAsync_WrongEmail_ThrowsUnauthorized()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

        var act = async () => await _service.LoginAsync(new LoginDto
        {
            Email    = "nobody@example.com",
            Password = "Password1",
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id           = 1,
            Email        = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1"),
        };

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
                     .ReturnsAsync(user);

        var act = async () => await _service.LoginAsync(new LoginDto
        {
            Email    = "test@example.com",
            Password = "WrongPassword1",
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── Update Profile ────────────────────────────────
    [Fact]
    public async Task UpdateProfileAsync_ChangeUsername_UpdatesSuccessfully()
    {
        var user = new User
        {
            Id           = 1,
            Username     = "oldname",
            Email        = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UsernameExistsAsync("newname"))
                     .ReturnsAsync(false);
        _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => u);

        var result = await _service.UpdateProfileAsync(1, new UpdateUserDto
        {
            Username = "newname",
        });

        result.Username.Should().Be("newname");
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_WrongCurrentPassword_ThrowsUnauthorized()
    {
        var user = new User
        {
            Id           = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1"),
        };

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        var act = async () => await _service.UpdateProfileAsync(1, new UpdateUserDto
        {
            CurrentPassword = "WrongPassword1",
            NewPassword     = "NewPassword1",
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}