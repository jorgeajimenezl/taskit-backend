using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Taskit.Web.Infrastructure;
using Taskit.Web.GitHub;

namespace Microsoft.Extensions.DependencyInjection;

public class LowercaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        return value?.ToString()?.ToLowerInvariant();
    }
}

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
        })
        .AddNewtonsoftJson();

        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 60,
                        QueueLimit = 0,
                        Window = TimeSpan.FromMinutes(1)
                    }
                )
            );
        });

        builder.Services.AddHttpClient<GitHubAuthService>();

        builder.Services.AddOpenIddict()
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();

                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                options.UseSystemNetHttp();
                options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough();

                options.UseWebProviders()
                    .AddGitHub(github =>
                    {
                        var configuration = builder.Configuration.GetSection("GitHub");
                        github.SetClientId(configuration["ClientId"]!)
                              .SetClientSecret(configuration["ClientSecret"]!)
                              .SetRedirectUri(configuration["RedirectUri"]!);
                    });
            });
    }
}