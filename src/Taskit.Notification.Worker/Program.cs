using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
        x.SetKebabCaseEndpointNameFormatter();

        x.SetInMemorySagaRepositoryProvider();

        var entryAssembly = Assembly.GetEntryAssembly();

        x.AddConsumers(entryAssembly);
        x.AddSagaStateMachines(entryAssembly);
        x.AddSagas(entryAssembly);
        x.AddActivities(entryAssembly);

        x.UsingInMemory((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);
        });
    });
});

var host = builder.Build();
host.Run();