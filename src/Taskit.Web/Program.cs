var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

app.UseHealthChecks("/health");
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseCors(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskIt API V1");
        config.RoutePrefix = string.Empty;
    });
}

app.UseExceptionHandler(options => { });
app.UseRateLimiter();

// Set up authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map the controllers
app.MapControllers();
app.UseHttpsRedirection();

app.Run();

