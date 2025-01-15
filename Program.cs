using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PostService.AsyncDataServices;
using PostService.Data;
using PostService.EventProcessor;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddHostedService<MessagebusSubscriber>();
builder.Services.AddScoped<IPostRepo, PostRepo>();
builder.Services.AddSingleton<IMesssageBusClient, MessageBusClient>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
if (builder.Environment.IsProduction())
{
    
    var dbUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
    var dbHost = Environment.GetEnvironmentVariable("POSTGRES_HOST");
    var dbPort = Environment.GetEnvironmentVariable("POSTGRES_PORT");
    var dbPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
    if (string.IsNullOrEmpty(dbUser) || string.IsNullOrEmpty(dbHost) || string.IsNullOrEmpty(dbPort) ||
        string.IsNullOrEmpty(dbPassword))
    {
        
        Console.WriteLine("One of the string values for Postgres are empty");
        Console.WriteLine($"Host={dbHost};Port={dbPort};Database=Posts;Username={dbUser};Password={dbPassword};Trust Server Certificate=true;");
        
    }
    builder.Configuration["ConnectionStrings:PostgressConn"] = $"Host={dbHost};Port={dbPort};Database=Users;Username={dbUser};Password={dbPassword};Trust Server Certificate=true;";
    
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgressConn")));
    
    builder.Configuration["Keycloak:ClientId"] = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENTID");
    builder.Configuration["Keycloak:ClientSecret"] = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENTSECRET");
    
    builder.Configuration["Keycloak:Authority"] = Environment.GetEnvironmentVariable("KEYCLOAK_AUTHORITY");
    builder.Configuration["Keycloak:Audience"] = Environment.GetEnvironmentVariable("KEYCLOAK_AUDIENCE");
    builder.Configuration["Keycloak:AuthenticationURL"] = Environment.GetEnvironmentVariable("KEYCLOAK_AUTHENTICATION_URL");
}
else
{
    Console.WriteLine("--> Using PostgreSQL Server");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgressConn")));
    
    // builder.Services.AddDbContext<AppDbContext>(opt => 
    //     opt.UseInMemoryDatabase("InMem")); 
}
builder.Services.AddAuthorization();
var integrationMode = builder.Configuration.GetValue<bool>("IntegrationMode");
if (!integrationMode)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        Console.WriteLine("---> Using Keycloak stuff");
        Console.WriteLine(builder.Configuration["Keycloak:Authority"]);
        Console.WriteLine(builder.Configuration["Keycloak:Audience"]);
    
        options.Authority = builder.Configuration["Keycloak:Authority"]; // Keycloak realm URL
        options.Audience = builder.Configuration["Keycloak:Audience"];   // Client ID
        options.RequireHttpsMetadata = false;            // Disable for development
    
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Keycloak:Authority"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Keycloak:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        }; 
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"Token challenge triggered: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    
    }); 
}

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
}
app.UseRouting();
app.MapControllers(); 
app.MapMetrics();
app.UseHttpsRedirection();

PrepDb.PrepPopulations(app, builder.Environment.IsProduction());
app.Run();


