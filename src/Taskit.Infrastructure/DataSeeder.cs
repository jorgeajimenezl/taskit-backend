using Microsoft.AspNetCore.Identity;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure;

public class DataSeeder(
    AppDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<AppUser> userManager)
{
    private readonly AppDbContext _context = context;
    private readonly RoleManager<IdentityRole> _roles = roleManager;
    private readonly UserManager<AppUser> _users = userManager;

    public async Task SeedAsync()
    {
        string[] defaultRoles = ["Admin", "UserManager", "User"];
        foreach (var role in defaultRoles)
        {
            if (!await _roles.RoleExistsAsync(role))
                await _roles.CreateAsync(new IdentityRole(role));
        }

        const string adminEmail = "admin@taskit.com";
        const string adminUserName = "admin";
        const string adminPassword = "Admin123!";

        var admin = await _users.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                FullName = "Administrator"
            };

            var result = await _users.CreateAsync(admin, adminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));
        }

        foreach (var role in defaultRoles)
        {
            if (!await _users.IsInRoleAsync(admin, role))
                await _users.AddToRoleAsync(admin, role);
        }
    }

    public void Seed()
    {
        SeedAsync().GetAwaiter().GetResult();
    }
}