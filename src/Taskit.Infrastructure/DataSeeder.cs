using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Taskit.Domain.Entities;

namespace Taskit.Infrastructure;

public class DataSeeder(
    AppDbContext context,
    RoleManager<IdentityRole> roleManager,
    UserManager<AppUser> userManager,
    ILogger<DataSeeder> logger)
{
    private readonly AppDbContext _context = context;
    private readonly RoleManager<IdentityRole> _roles = roleManager;
    private readonly UserManager<AppUser> _users = userManager;
    private readonly ILogger<DataSeeder> _logger = logger;

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding");
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

        await AddSampleUsersAsync();

        _logger.LogInformation("Database seeding completed");
    }

    public async Task AddSampleUsersAsync()
    {
        var sampleUsers = new List<AppUser>
        {
            new AppUser { UserName = "user1", Email = "user1@taskit.com", FullName = "User One" },
            new AppUser { UserName = "user2", Email = "user2@taskit.com", FullName = "User Two" },
            new AppUser { UserName = "user3", Email = "user3@taskit.com", FullName = "User Three" },
            new AppUser { UserName = "user4", Email = "user4@taskit.com", FullName = "User Four" },
        };
        foreach (var user in sampleUsers)
        {
            if (await _users.FindByEmailAsync(user.Email!) == null)
            {
                var result = await _users.CreateAsync(user, "User123!");
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }
    }

    public void Seed()
    {
        SeedAsync().GetAwaiter().GetResult();
    }
}