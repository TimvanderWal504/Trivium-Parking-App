using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models
{
    // Join table for Many-to-Many relationship between User and Role
    public class UserRole
    {
        [Key] // Composite Key defined in DbContext
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!; // Navigation property

        [Key] // Composite Key defined in DbContext
        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!; // Navigation property
    }
}