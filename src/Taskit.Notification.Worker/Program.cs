using System.Reflection;
using MassTransit;
using Taskit.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{
    services.AddDbContext<AppDbContext>(options =>
    {
        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlite(connectionString);
    });

    services.AddMassTransit(x =>
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        x.AddConsumers(entryAssembly);

        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
});

var host = builder.Build();
host.Run();