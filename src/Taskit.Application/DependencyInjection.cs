using System.Reflection;
using Gridify;
using Microsoft.Extensions.Hosting;
using Taskit.Application.Common.Settings;
using Taskit.Application.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        // Configure Gridify global settings
        GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

        // Register settings
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(Assembly.GetExecutingAssembly());
        });
        builder.Services.AddHttpContextAccessor();

        // Custom services
        builder.Services.AddScoped<TaskService>();
        builder.Services.AddScoped<ProjectService>();
        builder.Services.AddScoped<ProjectMemberService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<MediaService>();
        builder.Services.AddScoped<TaskCommentService>();
        builder.Services.AddScoped<TagService>();
        builder.Services.AddScoped<ProjectActivityLogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<UserService>();
    }
}