using WeatherApp.Models;

namespace WeatherApp.API.Repositories.Interfaces;

public interface IWeatherRepository : IGenericRepository<Weatherlog>
{
    Task<IEnumerable<Weatherlog>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Weatherlog>> GetRecentByUserIdAsync(int userId, int count = 10);
    Task<Weatherlog?> GetLastSearchAsync(int userId, string city);
    Task DeleteAllByUserIdAsync(int userId);
}