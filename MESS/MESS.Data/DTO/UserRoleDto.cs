using MESS.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace MESS.Data.DTO;

public class UserRoleDto
{
    public required ApplicationUser User { get; set; }
    public IList<string>? Roles { get; set; }
}