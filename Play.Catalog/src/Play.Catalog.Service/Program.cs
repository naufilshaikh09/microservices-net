using Microsoft.AspNetCore.Authentication.JwtBearer;
using Play.Catalog.Service;
using Play.Catalog.Service.Entities;
using Play.Common.Identity;
using Play.Common.MongoDB;
using Play.Common.MassTransit;
using Play.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items")
    .AddMassTransitWithRabbitMq()
    .AddJwtBearerAuthentication();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.Read, policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog.readaccess", "catalog.fullaccess");
    });
    
    options.AddPolicy(Policies.Write, policy =>
    {
        policy.RequireRole("Admin");
        policy.RequireClaim("scope", "catalog.writeaccess", "catalog.fullaccess");
    });
});

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
