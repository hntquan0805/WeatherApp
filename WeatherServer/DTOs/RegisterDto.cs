using System.ComponentModel.DataAnnotations;

namespace WeatherApp.API.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Username là bắt buộc")]
    [StringLength(50, MinimumLength = 3,
        ErrorMessage = "Username phải từ 3 đến 50 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password là bắt buộc")]
    [StringLength(100, MinimumLength = 6,
        ErrorMessage = "Password phải từ 6 ký tự trở lên")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password cần có chữ hoa, chữ thường và số")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Xác nhận password là bắt buộc")]
    [Compare("Password", ErrorMessage = "Password không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}