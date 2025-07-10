using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;

namespace Taskit.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<AppTask> Tasks { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
        base(options)
    { }
}