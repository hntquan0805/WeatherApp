namespace WeatherApp.API.DTOs;

// Trả về sau khi login/register thành công
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}