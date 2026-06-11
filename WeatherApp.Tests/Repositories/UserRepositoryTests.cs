using FluentAssertions;
using WeatherApp.Models;
using WeatherApp.API.Repositories;
using WeatherApp.Tests.Helpers;

namespace WeatherApp.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly WeatherContext   _context;
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        // Mỗi test class có DB riêng
        _context = DbContextHelper.Create(Guid.NewGuid().ToString());
        _repo    = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── Seed helper ───────────────────────────────────
    private async Task<User> SeedUserAsync(
        string username = "testuser",
        string email    = "test@example.com")
    {
        var user = new User
        {
            Username     = username,
            Email        = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
            Role         = "User",
        };
        return await _repo.AddAsync(user);
    }

    // ── GetById ───────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsUser()
    {
        var seeded = await SeedUserAsync();

        var result = await _repo.GetByIdAsync(seeded.Id);

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── GetByEmail ────────────────────────────────────
    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
    {
        await SeedUserAsync();

        var result = await _repo.GetByEmailAsync("test@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
    {
        await SeedUserAsync();

        var result = await _repo.GetByEmailAsync("TEST@EXAMPLE.COM");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_NonExistingEmail_ReturnsNull()
    {
        var result = await _repo.GetByEmailAsync("nobody@example.com");

        result.Should().BeNull();
    }

    // ── EmailExists ───────────────────────────────────
    [Fact]
    public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        await SeedUserAsync();

        var result = await _repo.EmailExistsAsync("test@example.com");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_NonExistingEmail_ReturnsFalse()
    {
        var result = await _repo.EmailExistsAsync("nobody@example.com");

        result.Should().BeFalse();
    }

    // ── Add ───────────────────────────────────────────
    [Fact]
    public async Task AddAsync_ValidUser_PersistsToDb()
    {
        var user = new User
        {
            Username     = "newuser",
            Email        = "new@example.com",
            PasswordHash = "hash",
            Role         = "User",
        };

        var result = await _repo.AddAsync(user);

        result.Id.Should().BeGreaterThan(0);
        var fromDb = await _repo.GetByIdAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Username.Should().Be("newuser");
    }

    // ── Update ────────────────────────────────────────
    [Fact]
    public async Task UpdateAsync_ExistingUser_UpdatesInDb()
    {
        var seeded = await SeedUserAsync();
        seeded.Username = "updateduser";

        await _repo.UpdateAsync(seeded);

        var fromDb = await _repo.GetByIdAsync(seeded.Id);
        fromDb!.Username.Should().Be("updateduser");
    }

    // ── Delete ────────────────────────────────────────
    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesFromDb()
    {
        var seeded = await SeedUserAsync();

        await _repo.DeleteAsync(seeded.Id);

        var fromDb = await _repo.GetByIdAsync(seeded.Id);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_DoesNotThrow()
    {
        var act = async () => await _repo.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }
}