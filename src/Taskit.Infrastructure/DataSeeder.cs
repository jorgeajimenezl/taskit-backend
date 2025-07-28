using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
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

        await AddSampleUsersAsync();

        await AddSampleDataAsync();

        _logger.LogInformation("Database seeding completed");
    }

    public async Task AddSampleUsersAsync()
    {
        var sampleUsers = new List<AppUser>
        {
            new() { UserName = "user1", Email = "user1@taskit.com", FullName = "User One" },
            new() { UserName = "user2", Email = "user2@taskit.com", FullName = "User Two" },
            new() { UserName = "user3", Email = "user3@taskit.com", FullName = "User Three" },
            new() { UserName = "user4", Email = "user4@taskit.com", FullName = "User Four" },
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

    public async Task AddSampleDataAsync()
    {
        if (await _context.Projects.AnyAsync())
            return;

        var user1 = await _users.FindByNameAsync("user1");
        var user2 = await _users.FindByNameAsync("user2");
        var user3 = await _users.FindByNameAsync("user3");
        var user4 = await _users.FindByNameAsync("user4");

        if (user1 == null || user2 == null || user3 == null || user4 == null)
            return;

        var tags = new List<TaskTag>
        {
            new() { Name = "bug", Color = "#dc2626" },
            new() { Name = "feature", Color = "#16a34a" },
            new() { Name = "docs", Color = "#2563eb" }
        };
        _context.TaskTags.AddRange(tags);
        await _context.SaveChangesAsync();

        var project1 = new Project
        {
            Name = "Demo project",
            Description = "Sample project created by seed data",
            OwnerId = user1.Id
        };

        var project2 = new Project
        {
            Name = "Secondary project",
            Description = "Another sample project",
            OwnerId = user2.Id
        };

        _context.Projects.AddRange(project1, project2);
        await _context.SaveChangesAsync();

        var members = new List<ProjectMember>
        {
            new() { ProjectId = project1.Id, UserId = user2.Id, Role = Domain.Enums.ProjectRole.Admin },
            new() { ProjectId = project1.Id, UserId = user3.Id, Role = Domain.Enums.ProjectRole.Member },
            new() { ProjectId = project2.Id, UserId = user1.Id, Role = Domain.Enums.ProjectRole.Member }
        };

        _context.ProjectMembers.AddRange(members);
        await _context.SaveChangesAsync();

        // Seed tasks with varied properties
        var task1 = new AppTask
        {
            Title = "Set up environment",
            Description = "Install dependencies and configure the project.",
            ProjectId = project1.Id,
            AuthorId = user1.Id,
            AssignedUserId = user2.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 2,
            CompletedPercentage = 50
        };

        var task2 = new AppTask
        {
            Title = "Fix login bug",
            Description = "Users cannot login with correct credentials.",
            ProjectId = project1.Id,
            AuthorId = user2.Id,
            AssignedUserId = user3.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 3,
            CompletedPercentage = 0
        };

        var task3 = new AppTask
        {
            Title = "Write documentation",
            Description = "Create user guide",
            ProjectId = project2.Id,
            AuthorId = user2.Id,
            AssignedUserId = user4.Id,
            Status = Domain.Enums.TaskStatus.Pending,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 1,
            CompletedPercentage = 20
        };

        var task4 = new AppTask
        {
            Title = "Implement user profile",
            Description = "Allow users to edit their profile details.",
            ProjectId = project1.Id,
            AuthorId = user3.Id,
            AssignedUserId = user1.Id,
            Status = Domain.Enums.TaskStatus.Created,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 2,
            CompletedPercentage = 0
        };

        var task5 = new AppTask
        {
            Title = "Add authentication tests",
            Description = "Cover login and registration with integration tests.",
            ProjectId = project1.Id,
            AuthorId = user1.Id,
            AssignedUserId = user3.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 3,
            CompletedPercentage = 40
        };

        var task6 = new AppTask
        {
            Title = "Design landing page",
            Description = "Create layout for the marketing page.",
            ProjectId = project2.Id,
            AuthorId = user4.Id,
            AssignedUserId = user2.Id,
            Status = Domain.Enums.TaskStatus.Completed,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 2,
            CompletedPercentage = 100
        };

        var task7 = new AppTask
        {
            Title = "Setup CI pipeline",
            Description = "Automate builds and tests on push.",
            ProjectId = project1.Id,
            AuthorId = user2.Id,
            AssignedUserId = user1.Id,
            Status = Domain.Enums.TaskStatus.Pending,
            Priority = Domain.Enums.TaskPriority.High,
            Complexity = 3,
            CompletedPercentage = 10
        };

        var task8 = new AppTask
        {
            Title = "Optimize database queries",
            Description = "Improve performance on heavy endpoints.",
            ProjectId = project1.Id,
            AuthorId = user3.Id,
            AssignedUserId = user2.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 4,
            CompletedPercentage = 60
        };

        var task9 = new AppTask
        {
            Title = "Create marketing materials",
            Description = "Prepare brochures and slides.",
            ProjectId = project2.Id,
            AuthorId = user1.Id,
            AssignedUserId = user4.Id,
            Status = Domain.Enums.TaskStatus.Created,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 1,
            CompletedPercentage = 0
        };

        var task10 = new AppTask
        {
            Title = "Refactor task module",
            Description = "Clean up task management components.",
            ProjectId = project1.Id,
            AuthorId = user4.Id,
            AssignedUserId = user3.Id,
            Status = Domain.Enums.TaskStatus.Cancelled,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 2,
            CompletedPercentage = 0
        };

        // Additional seed tasks
        var task11 = new AppTask
        {
            Title = "Conduct code review",
            Description = "Review codebase for best practices and consistency.",
            ProjectId = project1.Id,
            AuthorId = user3.Id,
            AssignedUserId = user2.Id,
            Status = Domain.Enums.TaskStatus.Pending,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 2,
            CompletedPercentage = 20
        };

        var task12 = new AppTask
        {
            Title = "Implement caching layer",
            Description = "Add Redis caching to improve response times.",
            ProjectId = project1.Id,
            AuthorId = user2.Id,
            AssignedUserId = user3.Id,
            Status = Domain.Enums.TaskStatus.Created,
            Priority = Domain.Enums.TaskPriority.High,
            Complexity = 3,
            CompletedPercentage = 0
        };

        var task13 = new AppTask
        {
            Title = "Design database schema",
            Description = "Create ER diagrams and define tables.",
            ProjectId = project2.Id,
            AuthorId = user4.Id,
            AssignedUserId = user1.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 4,
            CompletedPercentage = 50
        };

        var task14 = new AppTask
        {
            Title = "Setup monitoring tools",
            Description = "Integrate Application Insights and logging.",
            ProjectId = project2.Id,
            AuthorId = user1.Id,
            AssignedUserId = user4.Id,
            Status = Domain.Enums.TaskStatus.Pending,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 2,
            CompletedPercentage = 10
        };

        var task15 = new AppTask
        {
            Title = "Upgrade dependencies",
            Description = "Update NuGet packages to latest stable versions.",
            ProjectId = project1.Id,
            AuthorId = user1.Id,
            AssignedUserId = user2.Id,
            Status = Domain.Enums.TaskStatus.Completed,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 1,
            CompletedPercentage = 100
        };

        var task16 = new AppTask
        {
            Title = "Write API documentation",
            Description = "Document REST endpoints using Swagger.",
            ProjectId = project1.Id,
            AuthorId = user3.Id,
            AssignedUserId = user1.Id,
            Status = Domain.Enums.TaskStatus.Created,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 2,
            CompletedPercentage = 0
        };

        var task17 = new AppTask
        {
            Title = "Implement file upload",
            Description = "Allow users to attach files to tasks.",
            ProjectId = project2.Id,
            AuthorId = user4.Id,
            AssignedUserId = user2.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.High,
            Complexity = 4,
            CompletedPercentage = 30
        };

        var task18 = new AppTask
        {
            Title = "Fix UI bugs",
            Description = "Address layout issues in the web client.",
            ProjectId = project2.Id,
            AuthorId = user2.Id,
            AssignedUserId = user3.Id,
            Status = Domain.Enums.TaskStatus.InProgress,
            Priority = Domain.Enums.TaskPriority.Medium,
            Complexity = 2,
            CompletedPercentage = 0
        };

        var task19 = new AppTask
        {
            Title = "Optimize images",
            Description = "Compress and optimize images in assets.",
            ProjectId = project1.Id,
            AuthorId = user3.Id,
            AssignedUserId = user4.Id,
            Status = Domain.Enums.TaskStatus.Pending,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 1,
            CompletedPercentage = 0
        };

        var task20 = new AppTask
        {
            Title = "Finalize release notes",
            Description = "Compile and finalize release notes for deployment.",
            ProjectId = project2.Id,
            AuthorId = user2.Id,
            AssignedUserId = user1.Id,
            Status = Domain.Enums.TaskStatus.Created,
            Priority = Domain.Enums.TaskPriority.Low,
            Complexity = 1,
            CompletedPercentage = 0
        };

        _context.Tasks.AddRange(task1, task2, task3, task4, task5, task6, task7, task8, task9, task10,
            task11, task12, task13, task14, task15, task16, task17, task18, task19, task20);
        await _context.SaveChangesAsync();

        // Tag assignments for all seeded tasks
        task1.Tags.Add(tags[1]);
        task2.Tags.Add(tags[0]);
        task3.Tags.Add(tags[2]);
        task4.Tags.Add(tags[1]);
        task5.Tags.Add(tags[2]);
        task6.Tags.Add(tags[1]);
        task7.Tags.Add(tags[1]);
        task8.Tags.Add(tags[0]);
        task9.Tags.Add(tags[2]);
        task10.Tags.Add(tags[1]);
        task11.Tags.Add(tags[0]);
        task12.Tags.Add(tags[1]);
        task13.Tags.Add(tags[2]);
        task14.Tags.Add(tags[0]);
        task15.Tags.Add(tags[1]);
        task16.Tags.Add(tags[2]);
        task17.Tags.Add(tags[0]);
        task18.Tags.Add(tags[1]);
        task19.Tags.Add(tags[2]);
        task20.Tags.Add(tags[0]);
        await _context.SaveChangesAsync();

        var comments = new List<TaskComment>
        {
            new() { TaskId = task1.Id, AuthorId = user2.Id, Content = "I'll start with the backend setup." },
            new() { TaskId = task1.Id, AuthorId = user1.Id, Content = "Make sure to document the process." },
            new() { TaskId = task2.Id, AuthorId = user3.Id, Content = "Working on fix." },
            new() { TaskId = task3.Id, AuthorId = user4.Id, Content = "First draft ready." },
            new() { TaskId = task4.Id, AuthorId = user1.Id, Content = "Looking forward to implementing this." },
            new() { TaskId = task5.Id, AuthorId = user3.Id, Content = "Tests are crucial." },
            new() { TaskId = task6.Id, AuthorId = user2.Id, Content = "Need some inspiration." },
            new() { TaskId = task7.Id, AuthorId = user1.Id, Content = "CI pipeline will use GitHub Actions." },
            new() { TaskId = task8.Id, AuthorId = user4.Id, Content = "Investigating slow queries." },
            new() { TaskId = task9.Id, AuthorId = user2.Id, Content = "Marketing assets in progress." },
            new() { TaskId = task10.Id, AuthorId = user3.Id, Content = "Refactoring for better maintainability." },
        };
        _context.TaskComments.AddRange(comments);
        await _context.SaveChangesAsync();

        var media1 = new Media
        {
            Uuid = Guid.NewGuid(),
            CollectionName = "attachments",
            Name = "design.png",
            FileName = "design.png",
            MimeType = "image/png",
            Disk = "local",
            Size = 2048,
            AccessScope = AccessScope.Private,
            ModelId = task1.Id.ToString(),
            ModelType = nameof(AppTask),
            UploadedById = user1.Id
        };

        _context.Media.Add(media1);
        await _context.SaveChangesAsync();


        var log1 = new ProjectActivityLog
        {
            EventType = Domain.Enums.ProjectActivityLogEventType.ProjectCreated,
            UserId = user1.Id,
            ProjectId = project1.Id,
            Data = new Dictionary<string, object?>
            {
                ["projectId"] = project1.Id,
                ["name"] = project1.Name
            }
        };

        _context.ProjectActivityLogs.Add(log1);
        await _context.SaveChangesAsync();
    }

    public void Seed()
    {
        SeedAsync().GetAwaiter().GetResult();
    }
}