using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using Taskit.AI.Orchestrator.Consumers;
using Taskit.AI.Orchestrator;
using Taskit.AI.Orchestrator.Settings;
using Taskit.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString, o => o.UseVector()));

        services.Configure<SummaryGeneratorSettings>(
            context.Configuration.GetSection("Features:SummaryGeneration"));
        services.Configure<EmbeddingsGeneratorSettings>(
            context.Configuration.GetSection("Features:EmbeddingsGeneration"));

        var openAiKey = context.Configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(openAiKey))
            throw new ArgumentException("OpenAI API key is not configured.");
        services.AddSingleton(new OpenAIClient(openAiKey));

        services.AddScoped<MissingTaskEmbeddingsGenerator>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<SummaryGeneratorConsumer>();
            x.AddConsumer<TaskEmbeddingConsumer>();
            x.AddConsumer<RelatedTasksConsumer>();
            x.AddConsumer<DetectDuplicatesConsumer>();

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

if (args.Contains("--generate-missing-embeddings"))
{
    using var scope = host.Services.CreateScope();
    var generator = scope.ServiceProvider.GetRequiredService<MissingTaskEmbeddingsGenerator>();
    await generator.RunAsync();
    return;
}

await host.RunAsync();
