using System.ComponentModel.DataAnnotations;

namespace WeatherApp.API.DTOs;

// Nhận từ client khi tìm kiếm thời tiết
public class WeatherRequestDto
{
    [Required(ErrorMessage = "Tên thành phố là bắt buộc")]
    [StringLength(100, MinimumLength = 1)]
    public string City { get; set; } = string.Empty;

    // Đơn vị nhiệt độ: metric (°C) | imperial (°F) | standard (K)
    public string Units { get; set; } = "metric";
}