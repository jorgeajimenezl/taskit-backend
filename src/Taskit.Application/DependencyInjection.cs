using System.Reflection;
using Microsoft.Extensions.Hosting;
using Taskit.Application.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper((_) => Assembly.GetExecutingAssembly());
        builder.Services.AddHttpContextAccessor();

        // Custom services
        builder.Services.AddScoped<TaskService>();
        builder.Services.AddScoped<ProjectService>();
        builder.Services.AddScoped<ProjectMemberService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<MediaService>();
    }
}