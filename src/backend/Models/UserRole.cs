using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models;

public class UserRole
{
    [Key]
    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    [Key]
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;
}