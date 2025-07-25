using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Taskit.Infrastructure;
using Taskit.Web.Infrastructure;
using Taskit.Web.Settings;

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

        var githubOAuthSettings = builder.Configuration.GetSection("OAuth:GitHub")
            .Get<GithubOAuthSettings>()
            ?? throw new InvalidOperationException("GitHub OAuth settings are not configured.");

        builder.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>();
            })
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow();
                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough()
                    .EnablePostLogoutRedirectionEndpointPassthrough();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();

                options.UseSystemNetHttp();

                options.UseWebProviders()
                    .AddGitHub(gh =>
                    {
                        gh.SetClientId(githubOAuthSettings.ClientId);
                        gh.SetClientSecret(githubOAuthSettings.ClientSecret);
                        gh.SetRedirectUri(githubOAuthSettings.RedirectUri);
                        gh.AddScopes("read:user", "user:email");
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
    }
}