using GreenPipes;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Inventory.Service;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;

var allowedOriginSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddMongo()
    .AddMongoRepository<InventoryItem>("inventoryItems")
    .AddMongoRepository<CatalogItem>("catalogItems")
    .AddMassTransitWithRabbitMq(retryConfigurator =>
    {
        retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
        retryConfigurator.Ignore(typeof(UnknownItemException));
    })
    .AddJwtBearerAuthentication();

builder.AddCatalogClientConfig();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(builder =>
    {
        builder.WithOrigins(app.Configuration[allowedOriginSetting])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();