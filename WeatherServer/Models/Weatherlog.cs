using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WeatherApp.Models;

[Table("weatherlog")]
[Index("Id", Name = "IDX_WeatherLog_Id")]
[Index("UserId", Name = "IDX_WeatherLog_UserId")]
public partial class Weatherlog
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string City { get; set; } = null!;

    [StringLength(10)]
    public string Country { get; set; } = null!;

    /// <summary>
    /// Đơn vị: °C
    /// </summary>
    [Precision(5)]
    public decimal Temperature { get; set; }

    /// <summary>
    /// Đơn vị: °C
    /// </summary>
    [Precision(5)]
    public decimal FeelsLike { get; set; }

    /// <summary>
    /// Đơn vị: %  (0 - 100)
    /// </summary>
    public sbyte Humidity { get; set; }

    /// <summary>
    /// Đơn vị: m/s
    /// </summary>
    [Precision(6)]
    public decimal WindSpeed { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime SearchedAt { get; set; }

    public int? UserId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Weatherlogs")]
    public virtual User? User { get; set; }
}
