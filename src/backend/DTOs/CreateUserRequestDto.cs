using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.DTOs
{
    /// <summary>
    /// DTO for the request body when creating a new user.
    /// </summary>
    public class CreateUserRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
        public string? DisplayName { get; set; }

        // Optional: Include initial role if admins can set it during creation
        // [Required(ErrorMessage = "Initial role is required.")]
        // public string? InitialRole { get; set; } = "Employee"; // Default role
    }
}