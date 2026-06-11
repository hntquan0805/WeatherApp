using System.Net;
using System.Text.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using WeatherApp.Models;
using WeatherApp.API.Repositories.Interfaces;
using WeatherApp.API.Services;
using WeatherApp.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WeatherApp.Tests.Services;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherRepository>        _weatherRepoMock = new();
    private readonly Mock<IHttpClientFactory>        _httpFactoryMock = new();
    private readonly Mock<ILogger<WeatherService>>   _loggerMock      = new();
    private readonly IConfiguration                  _config;

    public WeatherServiceTests()
    {
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenWeatherMap:ApiKey"]  = "test-api-key",
                ["OpenWeatherMap:BaseUrl"] = "https://api.openweathermap.org/data/2.5",
            })
            .Build();
    }

    // Helper tạo service SAU KHI đã setup mock
    private WeatherService CreateService() => new WeatherService(
        _weatherRepoMock.Object,
        MapperHelper.Create(),
        _httpFactoryMock.Object,
        _config,
        _loggerMock.Object);

    // Helper: tạo fake HTTP response từ OpenWeatherMap
    private void SetupHttpClient(string city, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var fakeJson = statusCode == HttpStatusCode.OK
            ? JsonSerializer.Serialize(new
            {
                name = city,
                sys  = new { country = "VN", sunrise = 1700000000L, sunset = 1700043200L },
                main = new { temp = 30.5, feels_like = 33.0, temp_min = 28.0,
                             temp_max = 32.0, humidity = 75, pressure = 1010 },
                wind       = new { speed = 3.5 },
                weather    = new[] { new { description = "clear sky", icon = "01d" } },
                visibility = 10000,
            })
            : """{"cod":"404","message":"city not found"}""";

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content    = new StringContent(fakeJson),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        _httpFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>()))
                        .Returns(httpClient);
    }

    // ── GetWeather ────────────────────────────────────
    [Fact]
    public async Task GetWeatherAsync_ValidCity_ReturnsWeatherDto()
    {
        SetupHttpClient("Hanoi");

        var result = await CreateService().GetWeatherAsync("Hanoi");

        result.Should().NotBeNull();
        result.City.Should().Be("Hanoi");
        result.Country.Should().Be("VN");
        result.Temperature.Should().Be(30.5);
        result.Humidity.Should().Be(75);
        result.Unit.Should().Be("°C");
    }

    [Fact]
    public async Task GetWeatherAsync_InvalidCity_ThrowsKeyNotFound()
    {
        SetupHttpClient("InvalidCity999", HttpStatusCode.NotFound);

        var act = async () => await CreateService().GetWeatherAsync("InvalidCity999");

        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*InvalidCity999*");
    }

    [Fact]
    public async Task GetWeatherAsync_ImperialUnits_ReturnsCorrectUnit()
    {
        SetupHttpClient("Hanoi");

        var result = await CreateService().GetWeatherAsync("Hanoi", "imperial");

        result.Unit.Should().Be("°F");
    }

    // ── SaveAndGetWeather ─────────────────────────────
    [Fact]
    public async Task SaveAndGetWeatherAsync_ValidInput_SavesLog()
    {
        SetupHttpClient("Hanoi");
        _weatherRepoMock.Setup(r => r.AddAsync(It.IsAny<Weatherlog>()))
                        .ReturnsAsync(new Weatherlog { Id = 1 });

        await CreateService().SaveAndGetWeatherAsync("Hanoi", userId: 1);

        _weatherRepoMock.Verify(
            r => r.AddAsync(It.Is<Weatherlog>(l =>
                l.UserId == 1 && l.City == "Hanoi")),
            Times.Once);
    }

    // ── GetHistory ────────────────────────────────────
    [Fact]
    public async Task GetHistoryAsync_ReturnsLogDtos()
    {
        var logs = new List<Weatherlog>
        {
            new() { Id=1, UserId=1, City="Hanoi",  Country="VN",
                    Temperature=30, Humidity=70, Description="sunny",
                    Icon="01d", SearchedAt=DateTime.UtcNow },
            new() { Id=2, UserId=1, City="Danang", Country="VN",
                    Temperature=28, Humidity=65, Description="cloudy",
                    Icon="02d", SearchedAt=DateTime.UtcNow },
        };

        _weatherRepoMock.Setup(r => r.GetByUserIdAsync(1))
                        .ReturnsAsync(logs);

        var result = await CreateService().GetHistoryAsync(1);

        result.Should().HaveCount(2);
        result.Should().Contain(l => l.City == "Hanoi");
        result.Should().Contain(l => l.City == "Danang");
    }

    // ── DeleteHistory ─────────────────────────────────
    [Fact]
    public async Task DeleteHistoryAsync_CallsRepository()
    {
        _weatherRepoMock.Setup(r => r.DeleteAllByUserIdAsync(1))
                        .Returns(Task.CompletedTask);

        await CreateService().DeleteHistoryAsync(1);

        _weatherRepoMock.Verify(
            r => r.DeleteAllByUserIdAsync(1), Times.Once);
    }
}