var builder = WebApplication.CreateBuilder(args);
// var connectionString = builder.Configuration.GetConnectionString

// ("IdentityDataContextConnection") ?? throw new InvalidOperationException("Connection string 'IdentityDataContextConnection' not found.");

// builder.Services.AddDbContext<IdentityDataContext>(options => options.UseSqlServer(connectionString));

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<IdentityDataContext>();

// Add services to the container.

builder.Services.AddControllers();
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
