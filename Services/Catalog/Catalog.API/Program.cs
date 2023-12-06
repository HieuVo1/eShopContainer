using Catalog.API;
using Catalog.API.Extensions;
using Catalog.API.Infrastructure;
using HealthChecks.UI.Client;
using IntegrationEventLogEF;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddCustomDbContext(builder.Configuration)
        .AddIntegrationServices(builder.Configuration)
        .AddEventBus(builder.Configuration).
        AddCustomHealthCheck(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger()
 .UseSwaggerUI(c =>
 {
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
 });

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});


//app.Run();


try
{
    Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    var env = app.Services.GetService<IWebHostEnvironment>();
    var settings = app.Services.GetService<IOptions<CatalogSettings>>();
    var logger = app.Services.GetService<ILogger<CatalogContextSeed>>();
    await context.Database.MigrateAsync();

    await new CatalogContextSeed().SeedAsync(context, env, settings, logger);
    var integEventContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
    await integEventContext.Database.MigrateAsync();
    app.Logger.LogInformation("Starting web host ({ApplicationName})...", AppName);
    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
    public static string Namespace = typeof(Program).Assembly.GetName().Name;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}