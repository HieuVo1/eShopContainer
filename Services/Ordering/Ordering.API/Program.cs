using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Ordering.API;
using Ordering.API.Infrastructures;
using Ordering.API.Infrastructures.AutofacModules;
using Ordering.Infrastructure;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderingConnectionString"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        });
});

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(conbuilder => conbuilder.RegisterModule(new ApplicationModule()));
builder.Host.ConfigureContainer<ContainerBuilder>(conbuilder => conbuilder.RegisterModule(new MediatorModule()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    var env = app.Services.GetService<IWebHostEnvironment>();
    var settings = app.Services.GetService<IOptions<OrderingSettings>>();
    var logger = app.Services.GetService<ILogger<OrderingContextSeed>>();
    await context.Database.MigrateAsync();

    await new OrderingContextSeed().SeedAsync(context, env, settings, logger);

    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    return 1;
}
finally
{
}