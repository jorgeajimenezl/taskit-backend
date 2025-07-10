using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Taskit.Infrastructure;
using Taskit.Domain.Entities;
using Taskit.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureServices();
builder.AddWebServices();
builder.Services.AddOpenApi();

// Register custom services
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, DummyEmailSender>();

var app = builder.Build();

app.UseHealthChecks("/health");
app.UseRouting();

// Set up authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map the controllers
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskIt API V1");
        config.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.Run();

