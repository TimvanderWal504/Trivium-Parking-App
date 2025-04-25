using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.Models
{
    // Represents the user roles (Visitor, Management, Employee)
    public class Role
    {
        public int Id { get; set; } // Primary Key

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty; // e.g., "Visitor", "Management", "Employee"

        // Navigation property
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}