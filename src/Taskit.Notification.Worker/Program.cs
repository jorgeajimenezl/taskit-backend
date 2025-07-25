using System.Reflection;
using MassTransit;
using Taskit.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Infrastructure.Repositories;
using Taskit.Notification.Worker.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        // Register repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.Configure<EmailSettings>(context.Configuration.GetSection("Email"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailMessageGenerator, DefaultEmailMessageGenerator>();
        services.AddScoped<IRecipientResolver, DefaultRecipientResolver>();

        services.AddMassTransit(x =>
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            x.AddConsumers(entryAssembly);

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var host = context.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
                var username = context.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
                var password = context.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });
    })
    .Build();

host.Run();