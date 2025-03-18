using MESS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MESS.Data.Context;

public class UserContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public UserContext(DbContextOptions<UserContext> options) : base(options)
    {
    }
}