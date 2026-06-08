namespace WeatherApp.API.DTOs;

// Trả về cho client sau khi lấy từ OpenWeatherMap
public class WeatherDto
{
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public int Pressure { get; set; }
    public int Visibility { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;

    // URL icon để hiển thị trực tiếp trên React
    public string IconUrl => $"https://openweathermap.org/img/wn/{Icon}@2x.png";

    public DateTime Sunrise { get; set; }
    public DateTime Sunset { get; set; }
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public string Unit { get; set; } = "°C";
}