using System;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Taskit.Domain.Entities;
using Taskit.Domain.Enums;

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

        await SeedFromJsonAsync();

        _logger.LogInformation("Database seeding completed");
    }

    private record SeedData(
        List<SeedUser> Users,
        List<SeedTag> Tags,
        ProjectSeed Project,
        List<SeedTask> Tasks);

    private record SeedUser(string UserName, string Email, string FullName);
    private record SeedTag(string Name, string Color);
    private record ProjectSeed(string Name, string Description, string Owner);
    private record SeedComment(string Author, string Content, DateTime CreatedAt, DateTime UpdatedAt);
    private record SeedTask(
        string Title,
        string Description,
        string Status,
        string Priority,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string Author,
        string? Assigned,
        List<string> Tags,
        string Summary,
        List<SeedComment> Comments);

    private async Task SeedFromJsonAsync()
    {
        if (await _context.Projects.AnyAsync())
            return;

        var path = Path.Combine(AppContext.BaseDirectory, "SeedData", "seed-data.json");
        if (!File.Exists(path))
        {
            _logger.LogWarning("Seed data file not found at {Path}", path);
            return;
        }

        var json = await File.ReadAllTextAsync(path);
        var seed = JsonConvert.DeserializeObject<SeedData>(json);
        if (seed == null)
            return;

        foreach (var u in seed.Users)
        {
            if (await _users.FindByNameAsync(u.UserName) == null)
            {
                var user = new AppUser { UserName = u.UserName, Email = u.Email, FullName = u.FullName };
                var result = await _users.CreateAsync(user, "User123!");
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }
        await _context.SaveChangesAsync();

        foreach (var t in seed.Tags)
        {
            if (!await _context.TaskTags.AnyAsync(x => x.Name == t.Name))
            {
                _context.TaskTags.Add(new TaskTag { Name = t.Name, Color = t.Color });
            }
        }
        await _context.SaveChangesAsync();
        var tagMap = await _context.TaskTags.ToDictionaryAsync(x => x.Name);

        var owner = await _users.FindByNameAsync(seed.Project.Owner) ?? throw new InvalidOperationException("Owner user not found");
        var project = new Project
        {
            Name = seed.Project.Name,
            Description = seed.Project.Description,
            OwnerId = owner.Id
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        foreach (var u in seed.Users)
        {
            var user = await _users.FindByNameAsync(u.UserName);
            if (user != null)
            {
                _context.ProjectMembers.Add(new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = user.Id,
                    Role = u.UserName == seed.Project.Owner ? ProjectRole.Owner : ProjectRole.Member
                });
            }
        }
        await _context.SaveChangesAsync();

        foreach (var t in seed.Tasks)
        {
            var author = await _users.FindByNameAsync(t.Author);
            if (author == null)
                continue;
            var assignedUser = string.IsNullOrEmpty(t.Assigned) ? null : await _users.FindByNameAsync(t.Assigned);
            var task = new AppTask
            {
                Title = t.Title,
                Description = t.Description,
                GeneratedSummary = t.Summary,
                Status = Enum.Parse<TaskStatus>(t.Status),
                Priority = Enum.Parse<TaskPriority>(t.Priority),
                AuthorId = author.Id,
                AssignedUserId = assignedUser?.Id,
                ProjectId = project.Id,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };

            foreach (var tagName in t.Tags)
            {
                if (tagMap.TryGetValue(tagName, out var tag))
                    task.Tags.Add(tag);
            }

            foreach (var c in t.Comments)
            {
                var commentAuthor = await _users.FindByNameAsync(c.Author);
                if (commentAuthor != null)
                {
                    task.Comments.Add(new TaskComment
                    {
                        AuthorId = commentAuthor.Id,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    });
                }
            }

            _context.Tasks.Add(task);
        }

        await _context.SaveChangesAsync();
    }

    public void Seed()
    {
        SeedAsync().GetAwaiter().GetResult();
    }
}
