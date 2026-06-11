using FluentAssertions;
using WeatherApp.Models;
using WeatherApp.API.Repositories;
using WeatherApp.Tests.Helpers;

namespace WeatherApp.Tests.Repositories;

public class WeatherRepositoryTests : IDisposable
{
    private readonly WeatherContext       _context;
    private readonly WeatherRepository  _repo;
    private readonly User               _user;

    public WeatherRepositoryTests()
    {
        _context = DbContextHelper.Create(Guid.NewGuid().ToString());
        _repo    = new WeatherRepository(_context);

        // Seed user để làm FK
        _user = new User
        {
            Username     = "testuser",
            Email        = "test@example.com",
            PasswordHash = "hash",
            Role         = "User",
        };
        _context.Users.Add(_user);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    private Weatherlog MakeLog(string city = "Hanoi", int daysAgo = 0) => new()
    {
        UserId      = _user.Id,
        City        = city,
        Country     = "VN",
        Temperature = 30,
        Humidity    = 70,
        Description = "sunny",
        Icon        = "01d",
        SearchedAt  = DateTime.UtcNow.AddDays(-daysAgo),
    };

    // ── GetByUserId ───────────────────────────────────
    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyUserLogs()
    {
        await _repo.AddAsync(MakeLog("Hanoi"));
        await _repo.AddAsync(MakeLog("Danang"));

        var result = await _repo.GetByUserIdAsync(_user.Id);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(l => l.UserId == _user.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_OrderedBySearchedAtDesc()
    {
        await _repo.AddAsync(MakeLog("Hanoi",  daysAgo: 2));
        await _repo.AddAsync(MakeLog("Danang", daysAgo: 0));
        await _repo.AddAsync(MakeLog("HCM",    daysAgo: 1));

        var result = (await _repo.GetByUserIdAsync(_user.Id)).ToList();

        result[0].City.Should().Be("Danang");
        result[1].City.Should().Be("HCM");
        result[2].City.Should().Be("Hanoi");
    }

    // ── GetRecent ─────────────────────────────────────
    [Fact]
    public async Task GetRecentByUserIdAsync_ReturnsCorrectCount()
    {
        for (int i = 0; i < 15; i++)
            await _repo.AddAsync(MakeLog($"City{i}"));

        var result = await _repo.GetRecentByUserIdAsync(_user.Id, count: 10);

        result.Should().HaveCount(10);
    }

    // ── GetLastSearch ─────────────────────────────────
    [Fact]
    public async Task GetLastSearchAsync_ReturnsLatestForCity()
    {
        await _repo.AddAsync(MakeLog("Hanoi", daysAgo: 3));
        await _repo.AddAsync(MakeLog("Hanoi", daysAgo: 1));
        await _repo.AddAsync(MakeLog("Hanoi", daysAgo: 0));

        var result = await _repo.GetLastSearchAsync(_user.Id, "Hanoi");

        result.Should().NotBeNull();
        result!.SearchedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── DeleteAll ─────────────────────────────────────
    [Fact]
    public async Task DeleteAllByUserIdAsync_RemovesAllUserLogs()
    {
        await _repo.AddAsync(MakeLog("Hanoi"));
        await _repo.AddAsync(MakeLog("Danang"));

        await _repo.DeleteAllByUserIdAsync(_user.Id);

        var result = await _repo.GetByUserIdAsync(_user.Id);
        result.Should().BeEmpty();
    }
}