using Microsoft.AspNetCore.Identity;

namespace TriviumParkingApp.Backend.Models;

public class Role : IdentityRole<int>
{
    public int Priority { get; set; }

    public virtual ICollection<RoleParkingLot> RoleParkingLots { get; set; } = new List<RoleParkingLot>();
}