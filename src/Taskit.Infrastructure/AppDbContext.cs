using Newtonsoft.Json;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Taskit.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Taskit.Infrastructure;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<AppTask> Tasks { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Media> Media { get; set; }
    public DbSet<ProjectActivityLog> ProjectActivityLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) :
        base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NOTE: Fallback to JSON serialization for IDictionary properties
        // This is very slow, use with caution
        // Change it to a db that supports JSON natively (like PostgreSQL) for better performance
        var converter = new ValueConverter<IDictionary<string, object?>?, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, object?>>(v) ?? new Dictionary<string, object?>()
        );

        var dictionaryComparer = new ValueComparer<IDictionary<string, object?>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            v => v.Aggregate(0, (hash, p) =>
                HashCode.Combine(hash, p.Key.GetHashCode(), p.Value == null ? 0 : p.Value.GetHashCode())),
            v => v.ToDictionary(p => p.Key, kvp => kvp.Value)
        );

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppTask>()
            .HasOne(t => t.Author)
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppTask>()
            .HasOne(t => t.AssignedUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppTask>()
            .HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<AppTask>()
            .HasMany(t => t.Comments)
            .WithOne(c => c.Task)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AppTask>()
            .HasMany(t => t.Tags)
            .WithMany();

        modelBuilder.Entity<Project>()
            .HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(pm => pm.ProjectId);

        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Media>()
            .HasOne(m => m.UploadedBy)
            .WithMany();

        modelBuilder.Entity<Media>()
            .HasIndex(m => new { m.ModelId, m.ModelType });

        modelBuilder.Entity<Media>()
            .HasIndex(m => m.Uuid)
            .IsUnique();

        modelBuilder.Entity<Media>()
            .Property(m => m.Metadata)
            .HasColumnType("jsonb")
            .HasConversion(converter)
            .Metadata.SetValueComparer(dictionaryComparer);

        modelBuilder.Entity<ProjectActivityLog>()
            .Property(a => a.Data)
            .HasColumnType("jsonb")
            .HasConversion(converter)
            .Metadata.SetValueComparer(dictionaryComparer);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .Property(n => n.Data)
            .HasColumnType("jsonb")
            .HasConversion(converter)
            .Metadata.SetValueComparer(dictionaryComparer);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}