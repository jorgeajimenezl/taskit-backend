using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Taskit.Models;

namespace Taskit.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<AppTask> Tasks { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }
}