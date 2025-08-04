using System.Reflection;
using MassTransit;
using Taskit.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Interfaces;
using Taskit.Infrastructure.Repositories;
using Taskit.Notification.Worker.Services;
using Taskit.Notification.Worker.Interfaces;
using Taskit.Domain.Events;
using Taskit.Notification.Worker.Consumers;
using Taskit.Notification.Worker.Settings;
using Taskit.Notification.Worker.Services.MessageGenerators.Email;
using Taskit.Notification.Worker.Services.RecipientResolver;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.Configure<EmailSettings>(context.Configuration.GetSection("Email"));

        if (context.HostingEnvironment.IsDevelopment())
        {
            services.AddSingleton<IEmailSender, DummyEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        }

        // Register message generators
        services.AddScoped<IEmailMessageGenerator<ProjectActivityLogCreated>, ProjectActivityLogEmailMessageGenerator>();
        services.AddScoped<IRecipientResolver<ProjectActivityLogCreated>, ProjectActivityLogRecipientResolver>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<EmailNotificationConsumer<ProjectActivityLogCreated>>();

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