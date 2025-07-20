using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Taskit.Domain.Entities;
using Taskit.Infrastructure;
using Taskit.Infrastructure.Services;
using Taskit.Application.Interfaces;
using Taskit.Infrastructure.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlite(connectionString)
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    Console.WriteLine("Seeding database...");
                    var seeder = new DataSeeder(
                        (AppDbContext)context,
                        sp.GetRequiredService<RoleManager<IdentityRole>>(),
                        sp.GetRequiredService<UserManager<AppUser>>());
                    await seeder.SeedAsync();
                })
                .UseSeeding((context, _) =>
                {
                    Console.WriteLine("Seeding database...");
                    var seeder = new DataSeeder(
                        (AppDbContext)context,
                        sp.GetRequiredService<RoleManager<IdentityRole>>(),
                        sp.GetRequiredService<UserManager<AppUser>>());
                    seeder.Seed();
                });
        });

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        builder.Services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                )
            };
        });

        builder.Services.AddAuthorization();

        // Custom services
        builder.Services.AddSingleton<IEmailSender<AppUser>, DummyEmailSender>();
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<IMediaRepository, MediaRepository>();
        builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
    }
}
