using WeatherApp.API.DTOs;

namespace WeatherApp.API.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherDto> GetWeatherAsync(string city, string units = "metric");
    Task<WeatherDto> SaveAndGetWeatherAsync(string city, int userId, string units = "metric");
    Task<IEnumerable<WeatherLogDto>> GetHistoryAsync(int userId);
    Task<IEnumerable<WeatherLogDto>> GetRecentHistoryAsync(int userId, int count = 10);
    Task DeleteHistoryAsync(int userId);
}