using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.DTOs
{
    /// <summary>
    /// DTO for the assign role request body.
    /// </summary>
    public class AssignRoleRequestDto
    {
        [Required(ErrorMessage = "Role name is required.")]
        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters.")]
        public string? RoleName { get; set; }
    }
}