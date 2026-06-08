using System.ComponentModel.DataAnnotations;

namespace WeatherApp.API.DTOs;

public class UpdateUserDto
{
    [StringLength(50, MinimumLength = 3)]
    public string? Username { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    // Đổi password — tuỳ chọn
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 6)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password mới cần có chữ hoa, chữ thường và số")]
    public string? NewPassword { get; set; }
}