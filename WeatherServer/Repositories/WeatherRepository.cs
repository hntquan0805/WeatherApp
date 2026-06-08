using Microsoft.EntityFrameworkCore;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;

namespace WeatherApp.API.Repositories;

public class WeatherRepository : GenericRepository<Weatherlog>, IWeatherRepository
{
    public WeatherRepository(WeatherContext context) : base(context) { }

    public async Task<IEnumerable<Weatherlog>> GetByUserIdAsync(int userId) =>
        await _dbSet
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.SearchedAt)
            .ToListAsync();

    public async Task<IEnumerable<Weatherlog>> GetRecentByUserIdAsync(
        int userId, int count = 10) =>
        await _dbSet
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.SearchedAt)
            .Take(count)
            .ToListAsync();

    public async Task<Weatherlog?> GetLastSearchAsync(int userId, string city) =>
        await _dbSet
            .Where(w => w.UserId == userId
                     && w.City.ToLower() == city.ToLower())
            .OrderByDescending(w => w.SearchedAt)
            .FirstOrDefaultAsync();

    public async Task DeleteAllByUserIdAsync(int userId)
    {
        var logs = await _dbSet
            .Where(w => w.UserId == userId)
            .ToListAsync();

        _dbSet.RemoveRange(logs);
        await _context.SaveChangesAsync();
    }
}