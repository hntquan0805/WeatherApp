namespace WeatherApp.API.DTOs;

// Trả về lịch sử tìm kiếm của user
public class WeatherLogDto
{
    public int Id { get; set; }
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public int Humidity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IconUrl => $"https://openweathermap.org/img/wn/{Icon}@2x.png";
    public string Icon { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
}