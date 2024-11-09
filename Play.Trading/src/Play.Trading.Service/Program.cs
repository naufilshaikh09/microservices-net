using System.Reflection;
using System.Text.Json.Serialization;
using GreenPipes;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Identity.Contracts;
using Play.Inventory.Contracts;
using Play.Trading.Service.Entities;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.Settings;
using Play.Trading.Service.SignalR;
using Play.Trading.Service.StateMachines;

const string AllowedOriginSetting = "AllowedOrigin";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMongo()
    .AddMongoRepository<CatalogItem>("catalogItems")
    .AddMongoRepository<InventoryItem>("inventoryItems")
    .AddMongoRepository<ApplicationUser>("users")
    .AddJwtBearerAuthentication();

AddMassTransit(builder.Services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;   
})
.AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>()
    .AddSingleton<MessageHub>()
    .AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    app.UseCors(builder => builder
        .WithOrigins(app.Configuration[AllowedOriginSetting])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessageHub>("/messagehub");

app.Run();
return;

void AddMassTransit(IServiceCollection services)
{
    services.AddMassTransit(config =>
    {
        config.UsingPlayEconomyRabbitMq(retryConfigurator =>
        {
            retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
            retryConfigurator.Ignore(typeof(UnknownItemException));
        });
        
        config.AddConsumers(Assembly.GetEntryAssembly());
        config.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfigurator =>
            {
                sagaConfigurator.UseInMemoryOutbox();
            })
            .MongoDbRepository(r =>
            {
                var serviceSettings = builder.Configuration.GetSection(nameof(ServiceSettings))
                    .Get<ServiceSettings>();
                var mongoSettings = builder.Configuration.GetSection(nameof(MongoDbSettings))
                    .Get<MongoDbSettings>();

                r.Connection = mongoSettings.ConnectionString;
                r.DatabaseName = serviceSettings.ServiceName;
            });

        var queueSettings = builder.Configuration.GetSection(nameof(QueueSettings))
            .Get<QueueSettings>();
        
        EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
        EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGillQueueAddress));
        EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));
        
        services.AddMassTransitHostedService();
        services.AddGenericRequestClient();
    });
}