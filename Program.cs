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
// builder.Services.AddSingleton<IMesssageBusClient, MessageBusClient>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgressConn")));
}
else
{
    Console.WriteLine("--> Using PostgreSQL Server");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgressConn")));
    
    // builder.Services.AddDbContext<AppDbContext>(opt => 
    //     opt.UseInMemoryDatabase("InMem")); 
}
var app = builder.Build();



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


