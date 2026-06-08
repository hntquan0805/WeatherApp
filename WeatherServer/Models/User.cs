using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WeatherApp.Models;

[Table("user")]
[Index("Email", Name = "IDX_User_Email")]
[Index("Id", Name = "IDX_User_Id")]
[Index("Username", Name = "IDX_User_Username")]
[Index("Email", Name = "UQ_User_Email", IsUnique = true)]
[Index("Username", Name = "UQ_User_Username", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(20)]
    public string Role { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Weatherlog> Weatherlogs { get; set; } = new List<Weatherlog>();
}
