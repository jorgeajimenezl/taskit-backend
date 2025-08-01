using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OpenAI;
using Taskit.AI.Orchestrator.Consumers;
using Taskit.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

        var openAiKey = context.Configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(openAiKey))
        {
            services.AddSingleton(new OpenAIClient(openAiKey));
        }

        services.AddMassTransit(x =>
        {
            x.AddConsumer<AiSummaryConsumer>();

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
