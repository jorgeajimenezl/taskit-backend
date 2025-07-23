using System.Reflection;
using MassTransit;
using Taskit.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Infrastructure.Repositories;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        // Register repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddMassTransit(x =>
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            x.AddConsumers(entryAssembly);

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

host.Run();