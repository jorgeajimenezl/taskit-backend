using MassTransit;
using Microsoft.EntityFrameworkCore;
using Taskit.Infrastructure;
using Taskit.Notification.Worker;
using Taskit.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR();

builder.Services.AddSingleton<IEmailSender, StubEmailSender>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<RealtimeNotificationConsumer>();
    x.AddConsumer<EmailNotificationConsumer>();

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
