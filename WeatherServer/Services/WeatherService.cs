using System.Text.Json;
using AutoMapper;
using WeatherApp.API.DTOs;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;
using WeatherApp.API.Services.Interfaces;

namespace WeatherApp.API.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherRepository _weatherRepo;
    private readonly IMapper            _mapper;
    private readonly HttpClient         _httpClient;
    private readonly IConfiguration     _config;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IWeatherRepository      weatherRepo,
        IMapper                 mapper,
        IHttpClientFactory      httpClientFactory,
        IConfiguration          config,
        ILogger<WeatherService> logger)
    {
        _weatherRepo = weatherRepo;
        _mapper      = mapper;
        _httpClient  = httpClientFactory.CreateClient();
        _config      = config;
        _logger      = logger;
    }

    // ── Gọi OpenWeatherMap API ────────────────────────
    public async Task<WeatherDto> GetWeatherAsync(
        string city, string units = "metric")
    {
        var apiKey  = _config["OpenWeatherMap:ApiKey"];
        var baseUrl = _config["OpenWeatherMap:BaseUrl"];
        var url     = $"{baseUrl}/weather?q={Uri.EscapeDataString(city)}" +
                      $"&appid={apiKey}&units={units}&lang=vi";

        _logger.LogInformation("Gọi OpenWeatherMap cho city: {City}", city);

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Không tìm thấy thành phố: {City}", city);
            throw new KeyNotFoundException($"Không tìm thấy thành phố '{city}'.");
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json).RootElement;

        return ParseWeatherResponse(data, units);
    }

    // ── Lấy thời tiết + lưu lịch sử ──────────────────
    public async Task<WeatherDto> SaveAndGetWeatherAsync(
        string city, int userId, string units = "metric")
    {
        var weatherDto = await GetWeatherAsync(city, units);

        // Map WeatherDto → WeatherLog và lưu DB
        var log = _mapper.Map<Weatherlog>(weatherDto);
        log.UserId = userId;

        await _weatherRepo.AddAsync(log);

        return weatherDto;
    }

    // ── Lịch sử tìm kiếm ─────────────────────────────
    public async Task<IEnumerable<WeatherLogDto>> GetHistoryAsync(int userId)
    {
        var logs = await _weatherRepo.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<WeatherLogDto>>(logs);
    }

    public async Task<IEnumerable<WeatherLogDto>> GetRecentHistoryAsync(
        int userId, int count = 10)
    {
        var logs = await _weatherRepo.GetRecentByUserIdAsync(userId, count);
        return _mapper.Map<IEnumerable<WeatherLogDto>>(logs);
    }

    public async Task DeleteHistoryAsync(int userId) =>
        await _weatherRepo.DeleteAllByUserIdAsync(userId);

    // ── Parse JSON từ OpenWeatherMap ──────────────────
    private static WeatherDto ParseWeatherResponse(
        JsonElement data, string units)
    {
        var tempUnit = units switch
        {
            "imperial" => "°F",
            "standard" => "K",
            _          => "°C"
        };

        return new WeatherDto
        {
            City        = data.GetProperty("name").GetString()!,
            Country     = data.GetProperty("sys")
                              .GetProperty("country").GetString()!,
            Temperature = data.GetProperty("main")
                              .GetProperty("temp").GetDouble(),
            FeelsLike   = data.GetProperty("main")
                              .GetProperty("feels_like").GetDouble(),
            TempMin     = data.GetProperty("main")
                              .GetProperty("temp_min").GetDouble(),
            TempMax     = data.GetProperty("main")
                              .GetProperty("temp_max").GetDouble(),
            Humidity    = data.GetProperty("main")
                              .GetProperty("humidity").GetInt32(),
            Pressure    = data.GetProperty("main")
                              .GetProperty("pressure").GetInt32(),
            WindSpeed   = data.GetProperty("wind")
                              .GetProperty("speed").GetDouble(),
            Visibility  = data.TryGetProperty("visibility", out var vis)
                              ? vis.GetInt32() : 0,
            Description = data.GetProperty("weather")[0]
                              .GetProperty("description").GetString()!,
            Icon        = data.GetProperty("weather")[0]
                              .GetProperty("icon").GetString()!,
            Sunrise     = DateTimeOffset
                              .FromUnixTimeSeconds(
                                  data.GetProperty("sys")
                                      .GetProperty("sunrise").GetInt64())
                              .UtcDateTime,
            Sunset      = DateTimeOffset
                              .FromUnixTimeSeconds(
                                  data.GetProperty("sys")
                                      .GetProperty("sunset").GetInt64())
                              .UtcDateTime,
            SearchedAt  = DateTime.UtcNow,
            Unit        = tempUnit
        };
    }
}