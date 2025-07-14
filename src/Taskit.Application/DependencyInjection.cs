using System.Reflection;
using Microsoft.Extensions.Hosting;
using Taskit.Application.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAutoMapper((_) => Assembly.GetExecutingAssembly());

        // Custom services
        builder.Services.AddScoped<TaskService>();
        builder.Services.AddScoped<AuthService>();
    }
}