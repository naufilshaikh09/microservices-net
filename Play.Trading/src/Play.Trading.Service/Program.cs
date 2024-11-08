using System.Reflection;
using System.Text.Json.Serialization;
using GreenPipes;
using MassTransit;
using Play.Common.Identity;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Play.Common.Settings;
using Play.Identity.Contracts;
using Play.Inventory.Contracts;
using Play.Trading.Service.Entities;
using Play.Trading.Service.Exceptions;
using Play.Trading.Service.Settings;
using Play.Trading.Service.StateMachines;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddMongo()
    .AddMongoRepository<CatalogItem>("catalogItems")
    .AddJwtBearerAuthentication();
AddMassTransit(builder.Services);

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;   
})
.AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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
        
        EndpointConvention.Map<Contracts.GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));
        EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGillQueueAddress));
        
        services.AddMassTransitHostedService();
        services.AddGenericRequestClient();
    });
}