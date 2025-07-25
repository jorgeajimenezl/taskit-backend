using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Taskit.Domain.Entities;
using Taskit.Infrastructure;
using Taskit.Application.Interfaces;
using Taskit.Infrastructure.Repositories;
using MassTransit;
using Taskit.Infrastructure.Workers;

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
                    var logger = sp.GetRequiredService<ILogger<DataSeeder>>();
                    var seeder = new DataSeeder(
                        (AppDbContext)context,
                        sp.GetRequiredService<RoleManager<IdentityRole>>(),
                        sp.GetRequiredService<UserManager<AppUser>>(),
                        logger);
                    await seeder.SeedAsync();
                })
                .UseSeeding((context, _) =>
                {
                    var logger = sp.GetRequiredService<ILogger<DataSeeder>>();
                    var seeder = new DataSeeder(
                        (AppDbContext)context,
                        sp.GetRequiredService<RoleManager<IdentityRole>>(),
                        sp.GetRequiredService<UserManager<AppUser>>(),
                        logger);
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

        builder.Services.AddMassTransit(options =>
        {
            options.AddEntityFrameworkOutbox<AppDbContext>(cfg =>
            {
                cfg.UseSqlite();
                cfg.UseBusOutbox();
            });

            var host = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
            var username = builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
            var password = builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

            options.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        // Custom services
        builder.Services.AddScoped<ITaskRepository, TaskRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
        builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        builder.Services.AddScoped<IMediaRepository, MediaRepository>();
        builder.Services.AddScoped<ITaskCommentRepository, TaskCommentRepository>();
        builder.Services.AddScoped<ITagRepository, TagRepository>();
        builder.Services.AddScoped<IProjectActivityLogRepository, ProjectActivityLogRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
        builder.Services.AddScoped<IExternalLoginRepository, ExternalLoginRepository>();

        // Background services
        builder.Services.AddHostedService<MediaCleanupService>();
    }
}
