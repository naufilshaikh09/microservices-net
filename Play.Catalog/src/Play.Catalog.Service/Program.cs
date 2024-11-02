using MassTransit;
using MassTransit.Definition;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;
using Play.Common.MongoDB;
using Play.Service.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

builder.Services
    .AddMongo()
    .AddMongoRepository<Item>("items");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMQSettings = builder.Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
        cfg.Host(rabbitMQSettings.Host);
        cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
    });
});

builder.Services.AddMassTransitHostedService();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
